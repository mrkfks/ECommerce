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

// Load shared logging configuration if present
builder.Configuration.AddJsonFile("logging.common.json", optional: true, reloadOnChange: true);

// CORS origins from environment or config
var corsOrigins = (Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
    ?? builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:4200,http://localhost:5100,http://localhost:3000,https://your-frontend-onrender.com")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Serilog Configuration (read from configuration) and programmatic file sink to shared backend folder
var logDir = Environment.GetEnvironmentVariable("BACKEND_LOG_DIR")
    ?? Path.Combine(builder.Environment.ContentRootPath, "logs");
if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

var logFilePath = Path.Combine(logDir, "backend-log-.json");

var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext();

// Programmatically add a file sink that writes to the shared backend directory
loggerConfig = loggerConfig.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day);

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
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
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try 
    {
        logger.LogInformation("Starting Data Seeding...");
        var seeder = scope.ServiceProvider.GetRequiredService<ECommerce.Infrastructure.Data.DataSeeder>();
        await seeder.SeedAsync();
        logger.LogInformation("Data Seeding Completed Successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
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
app.UseCors("AllowFrontend");
app.UseMiddleware<ApiKeyMiddleware>();
app.UseRateLimiter();
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

// Ana sayfa - API bilgisi
app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>ECommerce API</title>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@300;400;600;700&display=swap' rel='stylesheet'>
    <style>
        :root {
            --primary: #4F46E5;
            --primary-hover: #4338ca;
            --bg-gradient-start: #f3f4f6;
            --bg-gradient-end: #e5e7eb;
            --card-bg: #ffffff;
            --text-main: #111827;
            --text-secondary: #6b7280;
            --success: #10B981;
        }

        body {
            font-family: 'Inter', sans-serif;
            margin: 0;
            padding: 0;
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            background: linear-gradient(135deg, #EEF2FF 0%, #E0E7FF 100%);
            color: var(--text-main);
        }

        .container {
            background: var(--card-bg);
            padding: 3rem;
            border-radius: 24px;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
            max-width: 500px;
            width: 90%;
            text-align: center;
            transition: transform 0.3s ease;
        }

        .container:hover {
            transform: translateY(-5px);
        }

        h1 {
            font-size: 2.5rem;
            margin-bottom: 0.5rem;
            background: linear-gradient(to right, #4F46E5, #7C3AED);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            font-weight: 800;
        }

        .status-badge {
            display: inline-flex;
            align-items: center;
            padding: 0.5rem 1rem;
            background-color: #D1FAE5;
            color: var(--success);
            border-radius: 9999px;
            font-weight: 600;
            font-size: 0.875rem;
            margin-bottom: 2rem;
            box-shadow: 0 0 0 1px #A7F3D0;
        }

        .status-dot {
            width: 8px;
            height: 8px;
            background-color: var(--success);
            border-radius: 50%;
            margin-right: 8px;
            animation: pulse 2s infinite;
        }

        p {
            color: var(--text-secondary);
            margin-bottom: 2rem;
            line-height: 1.6;
        }

        .actions {
            display: flex;
            flex-direction: column;
            gap: 1rem;
        }

        .btn {
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 1rem;
            text-decoration: none;
            border-radius: 12px;
            font-weight: 600;
            transition: all 0.2s ease;
            position: relative;
            overflow: hidden;
        }

        .btn-primary {
            background-color: var(--primary);
            color: white;
            box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2);
        }

        .btn-primary:hover {
            background-color: var(--primary-hover);
            transform: translateY(-2px);
            box-shadow: 0 6px 8px -1px rgba(79, 70, 229, 0.3);
        }

        .btn-secondary {
            background-color: white;
            color: var(--text-main);
            border: 1px solid #E5E7EB;
        }

        .btn-secondary:hover {
            background-color: #F9FAFB;
            border-color: #D1D5DB;
        }

        .endpoints {
            margin-top: 2rem;
            text-align: left;
            background: #F8FAFC;
            padding: 1.5rem;
            border-radius: 12px;
            border: 1px solid #F1F5F9;
        }

        .endpoints h3 {
            font-size: 0.875rem;
            text-transform: uppercase;
            letter-spacing: 0.05em;
            color: #64748B;
            margin-top: 0;
            margin-bottom: 1rem;
        }

        .endpoint-item {
            display: flex;
            align-items: center;
            margin-bottom: 0.75rem;
            font-size: 0.9rem;
            color: #334155;
        }

        .method {
            font-family: monospace;
            font-weight: 700;
            font-size: 0.75rem;
            padding: 0.2rem 0.5rem;
            border-radius: 4px;
            margin-right: 0.75rem;
            min-width: 50px;
            text-align: center;
        }

        .get { background: #DBEAFE; color: #1E40AF; }
        .post { background: #D1FAE5; color: #065F46; }

        @keyframes pulse {
            0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); }
            70% { box-shadow: 0 0 0 6px rgba(16, 185, 129, 0); }
            100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>ECommerce API</h1>
        <div class='status-badge'>
            <span class='status-dot'></span>
            Sistem Aktif & Çalışıyor
        </div>
        
        <p>E-Ticaret projesi için geliştirilmiş, yüksek performanslı RESTful API servisine hoş geldiniz.</p>

        <div class='actions'>
            <a href='/swagger' class='btn btn-primary'>
                <span>📖 Dokümantasyonu İncele (Swagger)</span>
            </a>
            <a href='/health' class='btn btn-secondary'>
                <span>❤️ Sistem Sağlığı (Health Check)</span>
            </a>
        </div>

        <div class='endpoints'>
            <h3>Öne Çıkan Endpointler</h3>
            <div class='endpoint-item'>
                <span class='method get'>GET</span>
                <span>/api/products - Ürün Listesi</span>
            </div>
            <div class='endpoint-item'>
                <span class='method get'>GET</span>
                <span>/api/categories - Kategoriler</span>
            </div>
            <div class='endpoint-item'>
                <span class='method post'>POST</span>
                <span>/api/auth/login - Kullanıcı Girişi</span>
            </div>
        </div>
    </div>
</body>
</html>
", "text/html"));

app.MapControllers();
app.MapHealthChecks("/health");

app.MapHub<ECommerce.Infrastructure.Hubs.NotificationHub>("/hub/notifications");
app.Logger.LogInformation("🚀 ECommerce API başlatıldı - http://localhost:5030");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"API başlatma hatası: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}
