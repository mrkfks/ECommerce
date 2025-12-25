using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Dashboard.Web.Infrastructure;
using Dashboard.Web.Services;
using Dashboard.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Base URL config
var apiBaseUrl = builder.Configuration.GetSection("ApiSettings")["BaseUrl"] ?? throw new InvalidOperationException("API BaseUrl not found");

// Add services to the container
builder.Services.AddControllersWithViews();

// HttpClient for API calls
builder.Services.AddHttpClient("ECommerceApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
})
    .AddHttpMessageHandler<AuthTokenHandler>();

// Typed HttpClient Services
builder.Services.AddHttpClient<CustomerApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<ProductApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<OrderApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<CategoryApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<BrandApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<ModelApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<CompanyApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<RequestApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<UserApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<ReviewApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
// AuthApiService doesn't need AuthTokenHandler (login/register endpoints)

// CORS (Angular veya başka frontend bağlanacaksa)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDashboard",
        policy => policy.WithOrigins("http://localhost:5041") // Dashboard domain
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();

// JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");
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
                return Task.CompletedTask;            },
            OnChallenge = context =>
            {
                // 401 Unauthorized ise Login page'e redirect et
                context.HandleResponse();
                context.Response.Redirect("/Auth/Login");
                return Task.CompletedTask;            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
        };
    });

// Authorization + rol bazlı yetkilendirme
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("CompanyAdminOrSuperAdmin", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin"));
    options.AddPolicy("CompanyAccess", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin", "CompanyStaff"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("CompanyAdmin", "SuperAdmin"));
    // Tüm sayfalar varsayılan olarak giriş gerektirsin
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Global Exception Handler
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // wwwroot için gerekli
app.UseRouting();

app.UseCors("AllowDashboard"); // CORS aktif
app.UseAuthentication();       // JWT aktif
app.UseAuthorization();        // Authorize attribute aktif

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

