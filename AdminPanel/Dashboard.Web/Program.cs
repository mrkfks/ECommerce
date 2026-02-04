using System.Text;
using Dashboard.Web.Infrastructure;
using Dashboard.Web.Middleware;
using Dashboard.Web.Models;
using Dashboard.Web.Services;
using ECommerce.Application.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;



// Base URL config - Environment variable veya appsettings'den
var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? (WebApplication.CreateBuilder().Configuration.GetSection("ApiSettings")["BaseUrl"])
    ?? throw new InvalidOperationException("API BaseUrl not found");

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<ApiService<GlobalAttributeFormDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddTransient<IApiService<GlobalAttributeFormDto>>(sp =>
    sp.GetRequiredService<ApiService<GlobalAttributeFormDto>>());
builder.Services.AddTransient<IApiService<CategoryFormDto>>(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("CategoryApi");
    return new ApiService<CategoryFormDto>(httpClient);
});


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


// CORS origins from environment or config
var dashboardCorsOrigins = (Environment.GetEnvironmentVariable("DASHBOARD_CORS_ALLOWED_ORIGINS")
    ?? "https://your-frontend-onrender.com,http://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

Console.WriteLine($"🔗 API Base URL: {apiBaseUrl}");
Log.Information($"[Startup] API Base URL: {apiBaseUrl}");

// Add services to the container
// Development: Runtime compilation for faster builds (no Razor compilation during build)
// Production: Precompiled views for better performance
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
}
else
{
    builder.Services.AddControllersWithViews();
}

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


// API Service registrations for each DTO type
builder.Services.AddHttpClient<IApiService<CategoryViewModel>, ApiService<CategoryViewModel>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();
// Each controller needs its specific API services registered with HttpClient

// CategoryController services

// BrandDto için HttpClient DI kaydı 'BrandApi' ismiyle sabitleniyor
builder.Services.AddHttpClient<IApiService<BrandDto>, ApiService<BrandDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// ModelDto için HttpClient DI kaydı 'ModelApi' ismiyle sabitleniyor
builder.Services.AddHttpClient<IApiService<ModelDto>, ApiService<ModelDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();


builder.Services.AddHttpClient<IApiService<GlobalAttributeDto>, ApiService<GlobalAttributeDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();


// ProductController services
builder.Services.AddHttpClient<IApiService<ProductViewModel>, ApiService<ProductViewModel>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// Removed duplicate BrandDto and CompanyDto registrations
// Using ECommerce.Application.DTOs.BrandDto and CompanyDto instead

// Other controllers
builder.Services.AddHttpClient<IApiService<RequestDto>, ApiService<RequestDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// OrderApiService - özel UpdateStatusAsync metodu için (OrderController)
builder.Services.AddHttpClient<OrderApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// IApiService<OrderDto> - HomeController ve diğer controller'lar için
builder.Services.AddHttpClient<IApiService<OrderDto>, ApiService<OrderDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<CustomerDto>, ApiService<CustomerDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<CompanyDto>, ApiService<CompanyDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// CompanyApi named client for company creation
builder.Services.AddHttpClient("CompanyApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<CategoryDto>, ApiService<CategoryDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<ProductDto>, ApiService<ProductDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<CampaignDto>, ApiService<CampaignDto>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<IApiService<BannerViewModel>, ApiService<BannerViewModel>>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

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


