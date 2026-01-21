using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using ECommerce.RestApi.Filters;
using ECommerce.RestApi.Middleware;
using ECommerce.RestApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers
    builder.Services.AddControllers(options =>
    {
        options.Filters.Add<ApiResponseFilter>();
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Data Protection - Anahtarları kalıcı dizinde sakla
    var keysDirectory = Environment.GetEnvironmentVariable("DOTNET_DATA_PROTECTION_KEY_DIRECTORY") 
                        ?? Path.Combine(builder.Environment.ContentRootPath, "keys");
    
    if (!Directory.Exists(keysDirectory))
    {
        Directory.CreateDirectory(keysDirectory);
    }

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
        .SetApplicationName("ECommerce");

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    // Caching
    builder.Services.AddResponseCaching();
    builder.Services.AddMemoryCache();

    // API Key Options
    builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKeys"));

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 1000,
                    QueueLimit = 2,
                    Window = TimeSpan.FromMinutes(1)
                }));
        
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
        };
    });

    // JWT Authentication
    var jwtConfig = builder.Configuration.GetSection("Jwt");
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? jwtConfig["Key"] ?? throw new InvalidOperationException("JWT Key bulunamadı");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtConfig["Issuer"];
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtConfig["Audience"];
    
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                ),
                ClockSkew = TimeSpan.Zero
            };
        });

    // Authorization
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("SuperAdminOnly", policy => 
            policy.RequireRole("SuperAdmin"));
        
        options.AddPolicy("CompanyAccess", policy => 
            policy.RequireRole("CompanyAdmin", "SuperAdmin", "User"));
        
        options.AddPolicy("SameCompanyOrSuperAdmin", policy =>
            policy.RequireAssertion(context =>
                context.User.IsInRole("SuperAdmin") ||
                context.User.HasClaim(c => c.Type == "CompanyId")));
    });

    // SignalR
    builder.Services.AddSignalR();

    // Cache (Default to Memory, change to Redis for prod)
    builder.Services.AddDistributedMemoryCache();
    // builder.Services.AddStackExchangeRedisCache(options => { options.Configuration = "localhost"; });

    builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, 
        ECommerce.RestApi.Authorization.SameCompanyAuthorizationHandler>();

    // Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ECommerce API",
            Version = "v1",
            Description = "ECommerce REST API Documentation"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Swagger için ek ayarlar
        c.UseInlineDefinitionsForEnums();
        c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
    });

    // Application & Infrastructure Services
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Database Migration & Seed
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<ECommerce.Infrastructure.Data.DataSeeder>();
        await seeder.SeedAsync();
    }

    // Middleware Pipeline
    // Always use our global exception handler so AppException status codes (e.g., 409 Conflict) surface correctly.
    // DeveloperExceptionPage masks custom status codes with 500, so we keep a single handler across environments.
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Swagger - hem Development hem Production'da açık
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API V1");
        c.RoutePrefix = "swagger";
    });

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseMiddleware<ApiKeyMiddleware>();
    app.UseRateLimiter();
    app.UseResponseCaching();

    app.UseAuthentication();
    app.UseAuthorization();

// Ana sayfa - API bilgisi
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html>
<head>
    <title>ECommerce API</title>
    <style>
        body { font-family: Arial, sans-serif; max-width: 800px; margin: 50px auto; padding: 20px; }
        h1 { color: #2c3e50; }
        .link { display: inline-block; margin: 10px 0; padding: 10px 20px; background: #3498db; color: white; text-decoration: none; border-radius: 5px; }
        .link:hover { background: #2980b9; }
        .status { color: #27ae60; font-weight: bold; }
    </style>
</head>
<body>
    <h1>🛒 ECommerce REST API</h1>
    <p class='status'>✅ API Çalışıyor</p>
    <p>Bu bir REST API servisidir. Aşağıdaki linkleri kullanabilirsiniz:</p>
    <a class='link' href='/swagger'>📖 Swagger API Dokümantasyonu</a><br>
    <a class='link' href='/health'>❤️ Health Check</a><br>
    <a class='link' href='/api/products'>📦 Ürünler API</a>
    <h3>Endpoints:</h3>
    <ul>
        <li><code>GET /api/products</code> - Ürün listesi</li>
        <li><code>GET /api/categories</code> - Kategori listesi</li>
        <li><code>GET /api/brands</code> - Marka listesi</li>
        <li><code>POST /api/auth/login</code> - Giriş</li>
    </ul>
</body>
</html>
", "text/html"));
    
app.MapControllers();
app.MapHealthChecks("/health");

app.MapHub<ECommerce.Infrastructure.Hubs.NotificationHub>("/hub/notifications");
app.Logger.LogInformation("🚀 ECommerce API başlatıldı - http://localhost:5010");
app.Run();
