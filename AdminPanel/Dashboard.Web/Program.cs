using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Dashboard.Web.Infrastructure;
using Dashboard.Web.Services;
using Dashboard.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Base URL config - Environment variable veya appsettings'den
var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") 
    ?? builder.Configuration.GetSection("ApiSettings")["BaseUrl"] 
    ?? throw new InvalidOperationException("API BaseUrl not found");

Console.WriteLine($"🔗 API Base URL: {apiBaseUrl}");

// Add services to the container
builder.Services.AddControllersWithViews();

// HttpClient for API calls
builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
    .AddHttpMessageHandler<AuthTokenHandler>();

// CORS (Angular veya başka frontend bağlanacaksa)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard",
        policy => policy.WithOrigins("http://localhost:5041") // Dashboard domain
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Generic API Service Registration
builder.Services.AddScoped(typeof(IApiService<>), typeof(ApiService<>));

// Custom Services for special logic (inheriting from ApiService or standalone)
builder.Services.AddScoped<CompanyApiService>();
builder.Services.AddScoped<CustomerApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<RequestApiService>();
builder.Services.AddScoped<OrderApiService>(); 
// Note: OrderApiService has UpdateStatusAsync. To fully remove, OrderController needs refactor.


// Other services that might need custom logic (Dashboard, Auth are different)
builder.Services.AddHttpClient<DashboardApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<UserManagementApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

// Registering simple services as just generic usage or if they are still classes:
// If CategoryController asks for CategoryApiService (class), it will fail if not registered.
// I need to change CategoryController to ask for IApiService<CategoryDto>.
// Or I can keep CategoryApiService class but update it to inherit new base and register it.
// Assuming I will update controllers to use IApiService<T>, I don't need to register CategoryApiService if I delete it.

// But wait, existing code uses CategoryApiService class. I should update ProductController and CategoryController.
// For safety, let's stick to the plan: Generic Service structure.

// Keep DashboardApService as Typed Client because it's not following CRUD pattern entirely (GetDashboardKpiAsync etc)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard",
        policy => policy.WithOrigins("http://localhost:5041") // Dashboard domain
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

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

// Development'ta HTTP kullanıldığı için HTTPS redirect kapalı
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles(); // wwwroot için gerekli
app.UseRouting();

app.UseCors("AllowDashboard"); // CORS aktif
app.UseAuthentication();       // JWT aktif
app.UseAuthorization();        // Authorize attribute aktif

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();


