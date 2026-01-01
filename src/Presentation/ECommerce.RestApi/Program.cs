using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using ECommerce.RestApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// Caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// JWT Authentication
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtConfig["Key"] ?? throw new InvalidOperationException("JWT Key bulunamadı"))
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Rol bazlı temel policy'ler
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("CompanyAdminOnly", policy =>
        policy.RequireRole("CompanyAdmin"));

    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireRole("Customer"));

    options.AddPolicy("CompanyAccess", policy =>
        policy.RequireRole("CompanyAdmin", "SuperAdmin", "User"));

    options.AddPolicy("CompanyAdminOrSuperAdmin", policy =>
        policy.RequireRole("CompanyAdmin", "SuperAdmin"));

    // Yetki bazlı policy'ler
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("SuperAdmin", "CompanyAdmin"));

    options.AddPolicy("CanViewAllCompanies", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("CanManageProducts", policy =>
        policy.RequireRole("SuperAdmin", "CompanyAdmin"));

    options.AddPolicy("CanManageOrders", policy =>
        policy.RequireRole("SuperAdmin", "CompanyAdmin"));

    options.AddPolicy("CanViewReports", policy =>
        policy.RequireRole("SuperAdmin", "CompanyAdmin"));

    options.AddPolicy("CanManageCampaigns", policy =>
        policy.RequireRole("SuperAdmin", "CompanyAdmin"));

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