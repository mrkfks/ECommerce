
using System.Text;
using Dashboard.Web.Infrastructure;
using Dashboard.Web.Middleware;
using Dashboard.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
// using ECommerce.Application; // Removed direct dependency on service registrations
// using ECommerce.Infrastructure; // Removed direct dependency on service registrations

var builder = WebApplication.CreateBuilder(args);

// Load shared logging configuration if present
builder.Configuration.AddJsonFile("logging.common.json", optional: true, reloadOnChange: true);

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

// Base URL config - Environment variable veya appsettings'den
var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? builder.Configuration.GetSection("ApiSettings")["BaseUrl"]
    ?? throw new InvalidOperationException("API BaseUrl not found");

// CORS origins from environment or config
var dashboardCorsOrigins = (Environment.GetEnvironmentVariable("DASHBOARD_CORS_ALLOWED_ORIGINS")
    ?? "https://your-frontend-onrender.com,http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

Console.WriteLine($"🔗 API Base URL: {apiBaseUrl}");

// Add services to the container
builder.Services.AddControllersWithViews();

// Infrastructure services (DbContext, Repositories, etc.) are NOT needed for Dashboard.Web
// as it communicates via API.
// builder.Services.AddInfrastructureServices(builder.Configuration);

// Application services (AutoMapper, FluentValidation, etc.) are NOT needed if not using local processing
// builder.Services.AddApplicationServices();

// SignalR for real-time notifications
builder.Services.AddSignalR();

// Response Caching for VaryByQueryKeys support
builder.Services.AddResponseCaching();

// HttpClient for API calls

builder.Services.AddHttpClient<CompanyApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// ReviewApiService DI kaydı (sadece burada ve apiBaseUrl'den sonra olmalı)
builder.Services.AddHttpClient<ReviewApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// CORS (Angular veya başka frontend bağlanacaksa)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard",
        policy => policy.WithOrigins("http://localhost:5001")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});


// Generic API Service Registration
builder.Services.AddScoped(typeof(IApiService<>), typeof(ApiService<>));

// Typed API Services with custom logic
builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<NotificationApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<LoginHistoryApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<UserManagementApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<CustomerMessageApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<DashboardApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddHttpClient<UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();

// JWT Authentication - Environment variable'dan veya appsettings'den oku
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? "ECommerce";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? "ECommerce.Client";

Console.WriteLine($"🔐 JWT configured - Issuer: {jwtIssuer}, Audience: {jwtAudience}");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Cookie’den token oku
                var token = context.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                // 401 Unauthorized ise Login page'e redirect et
                if (!context.Response.HasStarted)
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Auth/Login");
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                // Token validation failed
                if (context.Exception != null)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Authorization + rol bazlı yetkilendirme
builder.Services.AddAuthorization(options =>
{
    // Rol bazlı temel policy'ler
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdminOnly", policy => policy.RequireRole("CompanyAdmin"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("CompanyAdminOrSuperAdmin", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin"));
    options.AddPolicy("CompanyAccess", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin", "CompanyStaff"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin"));

    // Yetki bazlı policy'ler
    options.AddPolicy("CanManageUsers", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("CanViewAllCompanies", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CanManageProducts", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("CanManageOrders", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("CanViewReports", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("CanManageCampaigns", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    options.AddPolicy("CanViewLoginHistory", policy => policy.RequireRole("SuperAdmin", "CompanyAdmin"));
    // FallbackPolicy removed - Controllers have [Authorize] attributes
});

// Configure Helpers
Dashboard.Web.Helpers.ImageHelper.Configure(builder.Configuration);


var app = builder.Build();

// Global Exception Handler
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseStaticFiles();
app.UseRouting();
// Response Caching middleware, UseRouting'den sonra ve UseAuthorization'dan önce olmalı
app.UseResponseCaching();
app.UseCors("AllowDashboard");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");


try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Uygulama başlatma hatası: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
    throw;
}


