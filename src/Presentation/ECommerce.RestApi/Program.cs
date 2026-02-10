using System.Text;
using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Data;
using ECommerce.RestApi.Filters;
using ECommerce.RestApi.Middleware;
using ECommerce.RestApi.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// SignalR ve Distributed Cache servisleri
builder.Services.AddSignalR();
builder.Services.AddDistributedMemoryCache();

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

// Rate Limiting - Development için çok daha yüksek limit
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Environment.IsDevelopment() ? 10000 : 1000,
                QueueLimit = builder.Environment.IsDevelopment() ? 100 : 2,
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

// Database Migration & SuperAdmin Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Database migrations completed");

        // ========== SUPER ADMIN SEED ==========
        // Önce Company var mı kontrol et, yoksa oluştur
        var systemCompany = await context.Companies.FirstOrDefaultAsync(c => c.Name == "System");
        if (systemCompany == null)
        {
            systemCompany = ECommerce.Domain.Entities.Company.Create(
                name: "System",
                address: "System Address",
                phoneNumber: "0000000000",
                email: "system@ecommerce.com",
                taxNumber: "0000000000"
            );
            // Şirketi aktif ve onaylı yap
            systemCompany.Approve();

            // Localhost domainini ve renkleri ata
            systemCompany.UpdateBranding("localhost", "", "#3f51b5", "#f50057");

            context.Companies.Add(systemCompany);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ System company created with ID: {CompanyId}", systemCompany.Id);
        }
        else if (string.IsNullOrEmpty(systemCompany.Domain))
        {
            // Eğer domain atanmamışsa ata
            systemCompany.UpdateBranding("localhost", systemCompany.LogoUrl ?? "", systemCompany.PrimaryColor ?? "#3f51b5", systemCompany.SecondaryColor ?? "#f50057");
            await context.SaveChangesAsync();
            logger.LogInformation("✅ System company branding updated with domain 'localhost'");
        }

        // SuperAdmin rolü var mı kontrol et
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            superAdminRole = ECommerce.Domain.Entities.Role.Create("SuperAdmin", "Sistem yöneticisi - tüm yetkilere sahip");
            context.Roles.Add(superAdminRole);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ SuperAdmin role created");
        }

        // Admin rolü var mı kontrol et
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = ECommerce.Domain.Entities.Role.Create("Admin", "Şirket yöneticisi");
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Admin role created");
        }

        // User rolü var mı kontrol et
        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
        {
            userRole = ECommerce.Domain.Entities.Role.Create("User", "Standart kullanıcı");
            context.Roles.Add(userRole);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ User role created");
        }

        // CompanyAdmin rolü var mı kontrol et (Şirket kayıt için gerekli)
        var companyAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "CompanyAdmin");
        if (companyAdminRole == null)
        {
            companyAdminRole = ECommerce.Domain.Entities.Role.Create("CompanyAdmin", "Şirket yöneticisi - kendi şirketinin tüm yetkilerine sahip");
            context.Roles.Add(companyAdminRole);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ CompanyAdmin role created");
        }

        // Customer rolü var mı kontrol et (Genel müşteri kaydı için gerekli)
        var customerRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer");
        if (customerRole == null)
        {
            customerRole = ECommerce.Domain.Entities.Role.Create("Customer", "Genel müşteri - alışveriş yapabilir");
            context.Roles.Add(customerRole);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Customer role created");
        }

        // SuperAdmin kullanıcısı var mı kontrol et
        var superAdminEmail = "superadmin@ecommerce.com";
        var existingSuperAdmin = await context.Users.FirstOrDefaultAsync(u => u.Email == superAdminEmail);

        if (existingSuperAdmin == null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!");
            var superAdminUser = ECommerce.Domain.Entities.User.Create(
                companyId: systemCompany.Id,
                username: "superadmin",
                email: superAdminEmail,
                passwordHash: passwordHash,
                firstName: "Super",
                lastName: "Admin"
            );

            context.Users.Add(superAdminUser);
            await context.SaveChangesAsync();

            // Kullanıcıya SuperAdmin rolü ata
            var superAdminUserRole = ECommerce.Domain.Entities.UserRole.Create(
                userId: superAdminUser.Id,
                roleId: superAdminRole.Id,
                roleName: "SuperAdmin"
            );
            context.UserRoles.Add(superAdminUserRole);
            await context.SaveChangesAsync();

            logger.LogInformation("✅ SuperAdmin user created - Email: {Email}", superAdminEmail);
        }
        else
        {
            // Eğer mevcut kullanıcı kısmi bilgiyle kayıtlıysa eksik alanları güncelle
            var updated = false;

            if (string.IsNullOrWhiteSpace(existingSuperAdmin.Username) || existingSuperAdmin.Username != "superadmin"
                || string.IsNullOrWhiteSpace(existingSuperAdmin.FirstName) || existingSuperAdmin.FirstName != "Super"
                || string.IsNullOrWhiteSpace(existingSuperAdmin.LastName) || existingSuperAdmin.LastName != "Admin")
            {
                existingSuperAdmin.UpdateProfile("Super", "Admin", superAdminEmail, "superadmin", existingSuperAdmin.PhoneNumber);
                updated = true;
            }

            if (!existingSuperAdmin.IsActive)
            {
                existingSuperAdmin.Activate();
                updated = true;
            }

            // Kullanıcıya SuperAdmin rolü atanmış mı kontrol et, yoksa ata
            var hasSuperRole = await context.UserRoles.AnyAsync(ur => ur.UserId == existingSuperAdmin.Id && ur.RoleId == superAdminRole.Id);
            if (!hasSuperRole)
            {
                var superAdminUserRole = ECommerce.Domain.Entities.UserRole.Create(
                    userId: existingSuperAdmin.Id,
                    roleId: superAdminRole.Id,
                    roleName: "SuperAdmin"
                );
                context.UserRoles.Add(superAdminUserRole);
                updated = true;
            }

            if (updated)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("✅ SuperAdmin user updated - Email: {Email}", superAdminEmail);
            }
            else
            {
                logger.LogInformation("ℹ️ SuperAdmin user already exists");
            }
        }
        // ========== SUPER ADMIN SEED END ==========

        // ========== TEST DATA SEED ==========
        logger.LogInformation("🔍 Starting test data seed...");
        // Test kategorisi ve markası
        logger.LogInformation("🔍 Checking for Electronics category...");
        var testCategoryExists = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        logger.LogInformation("🔍 Category check result: {Result}", testCategoryExists == null ? "Not found" : "Found");
        var testCategory = testCategoryExists ?? ECommerce.Domain.Entities.Category.Create(
            name: "Electronics",
            description: "Electronic products",
            imageUrl: null,
            companyId: systemCompany.Id
        );
        if (testCategoryExists == null)
        {
            testCategory.Activate();
            context.Categories.Add(testCategory);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Category 'Electronics' created");
        }

        logger.LogInformation("🔍 Checking for TechBrand brand...");
        var testBrandExists = await context.Brands.FirstOrDefaultAsync(b => b.Name == "TechBrand");
        logger.LogInformation("🔍 Brand check result: {Result}", testBrandExists == null ? "Not found" : "Found");
        var testBrand = testBrandExists ?? ECommerce.Domain.Entities.Brand.Create(
            name: "TechBrand",
            description: "Technology brand",
            companyId: systemCompany.Id
        );
        if (testBrandExists == null)
        {
            context.Brands.Add(testBrand);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Brand 'TechBrand' created");
        }

        // Test ürünleri
        logger.LogInformation("🔍 Checking for test products...");
        try
        {
            var productsCount = await context.Products.AsNoTracking().CountAsync();
            logger.LogInformation("🔍 Product count: {Count}", productsCount);
            if (productsCount == 0)
            {
                var testProducts = new[]
                {
                    ECommerce.Domain.Entities.Product.Create("Laptop", "High-performance laptop", 5000m, testCategory.Id, testBrand.Id, systemCompany.Id, 10),
                    ECommerce.Domain.Entities.Product.Create("Mouse", "Wireless mouse", 150m, testCategory.Id, testBrand.Id, systemCompany.Id, 50),
                    ECommerce.Domain.Entities.Product.Create("Keyboard", "Mechanical keyboard", 300m, testCategory.Id, testBrand.Id, systemCompany.Id, 30),
                    ECommerce.Domain.Entities.Product.Create("Monitor", "4K Monitor", 2000m, testCategory.Id, testBrand.Id, systemCompany.Id, 20),
                    ECommerce.Domain.Entities.Product.Create("Headphones", "Bluetooth headphones", 500m, testCategory.Id, testBrand.Id, systemCompany.Id, 60)
                };
                context.Products.AddRange(testProducts);
                await context.SaveChangesAsync();
                logger.LogInformation("✅ Test products created");
            }
        }
        catch (Exception pex)
        {
            logger.LogError(pex, "❌ Error during product seed: {Message}", pex.Message);
        }

        // Test müşteriler ve adresler
        logger.LogInformation("🔍 Checking for test customers...");
        var customersCount = await context.Users.Where(u => u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Customer")).CountAsync();
        logger.LogInformation("🔍 Customers count: {Count}", customersCount);
        if (customersCount == 0)
        {
            for (int i = 1; i <= 5; i++)
            {
                var customerEmail = $"customer{i}@test.com";
                var customerExists = await context.Users.FirstOrDefaultAsync(u => u.Email == customerEmail);

                if (customerExists == null)
                {
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test123!");
                    var customer = ECommerce.Domain.Entities.User.Create(
                        companyId: systemCompany.Id,
                        username: $"customer{i}",
                        email: customerEmail,
                        passwordHash: passwordHash,
                        firstName: $"Test",
                        lastName: $"Customer{i}"
                    );
                    context.Users.Add(customer);
                    await context.SaveChangesAsync();

                    var customerRoleEntry = ECommerce.Domain.Entities.UserRole.Create(customer.Id, customerRole.Id, "Customer");
                    context.UserRoles.Add(customerRoleEntry);
                    await context.SaveChangesAsync();

                    // Müşteri (Customer) kaydı oluştur
                    var dbCustomer = ECommerce.Domain.Entities.Customer.Create(
                        companyId: systemCompany.Id,
                        firstName: $"Test",
                        lastName: $"Customer{i}",
                        email: customerEmail,
                        phoneNumber: $"05550000{i:D3}",
                        dateOfBirth: DateTime.UtcNow.AddYears(-30),
                        userId: customer.Id
                    );
                    context.Customers.Add(dbCustomer);
                    await context.SaveChangesAsync();

                    // Address oluştur
                    var address = ECommerce.Domain.Entities.Address.Create(
                        customerId: dbCustomer.Id,
                        street: $"Test Street {i}",
                        city: "Istanbul",
                        state: "Istanbul",
                        zipCode: $"3400{i}",
                        country: "Turkey"
                    );
                    context.Addresses.Add(address);
                    await context.SaveChangesAsync();
                }
            }
            logger.LogInformation("✅ Test customers created");
        }

        // Test siparişleri
        logger.LogInformation("🔍 Checking for test orders...");
        var ordersCount = await context.Orders.CountAsync();
        logger.LogInformation("🔍 Orders count: {Count}", ordersCount);
        if (ordersCount == 0)
        {
            var customers = await context.Customers
                .Include(c => c.Addresses)
                .Take(3)
                .ToListAsync();
            var products = await context.Products.Take(3).ToListAsync();

            foreach (var customer in customers)
            {
                if (products.Count > 0 && customer.Addresses != null && customer.Addresses.Count > 0)
                {
                    var address = customer.Addresses.First();
                    var order = ECommerce.Domain.Entities.Order.Create(
                        customerId: customer.Id,
                        addressId: address.Id,
                        companyId: systemCompany.Id
                    );
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    // Order items ekle
                    decimal totalAmount = 0;
                    foreach (var product in products)
                    {
                        var orderItem = ECommerce.Domain.Entities.OrderItem.Create(
                            productId: product.Id,
                            quantity: 2,
                            unitPrice: product.Price
                        );
                        order.Items.Add(orderItem);
                        totalAmount += product.Price * 2;
                    }

                    // OrderItem'ler eklendikten sonra UpdateTotalAmount metodu çağır
                    order.GetType().GetProperty("TotalAmount",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(order, totalAmount);

                    await context.SaveChangesAsync();
                }
            }
            logger.LogInformation("✅ Test orders created");
        }

        // Test kampanyaları
        logger.LogInformation("🔍 Checking for test campaigns...");
        var campaignsCount = await context.Campaigns.CountAsync();
        logger.LogInformation("🔍 Campaigns count: {Count}", campaignsCount);
        if (campaignsCount == 0)
        {
            var campaign = ECommerce.Domain.Entities.Campaign.Create(
                name: "Yılbaşı Kampanyası",
                discountPercent: 25,
                startDate: DateTime.UtcNow.AddDays(-5),
                endDate: DateTime.UtcNow.AddDays(25),
                companyId: systemCompany.Id,
                description: "Tüm ürünlerde %25 indirim"
            );
            context.Campaigns.Add(campaign);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Test campaign created");
        }
        // ========== TEST DATA SEED END ==========
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Migration/Seed error: {Message}", ex.Message);
        logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
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

// Static Files - wwwroot klasöründen dosya servisi
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // CORS headers for images
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

// Uploads klasöründen dosya servisi - /uploads path'ine map et
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // CORS headers for uploaded images
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET");
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=600");
    }
});

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

app.Logger.LogInformation("🚀 ECommerce API başlatıldı - http://localhost:5010");
app.Run();

// Expose Program class for integration tests
public partial class Program { }
