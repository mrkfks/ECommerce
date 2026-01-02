using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using ECommerce.RestApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
    builder.Services.AddControllers();

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
    });

    // Application & Infrastructure Services
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Database Migration
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
            logger.LogInformation("✅ Database migrations completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Migration error");
        }
    }

    // Middleware Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce API V1"));
    }
    else
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseCors("AllowAll");
    app.UseResponseCaching();

    app.UseAuthentication();
    app.UseAuthorization();
    
app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("🚀 ECommerce API başlatıldı - http://localhost:5010");
app.Run();