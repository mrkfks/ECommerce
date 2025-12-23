# Clean Architecture Refactoring Plan

## Proje Genel Durum
- **Framework:** .NET 10.0
- **Database:** SQLite (Development)
- **Test Framework:** xUnit 2.8.1, Moq 4.20.70, FluentAssertions
- **Frontend:** Angular

---

## Phase 1: Test Infrastructure ✅ COMPLETED

### 1.1 Unit Test Setup ✅
- [x] xUnit, Moq, FluentAssertions, Bogus paketleri eklendi
- [x] ProductTestDataBuilder oluşturuldu
- [x] CreateProductCommandHandlerTests yazıldı (18 test case)
- [x] Usings.cs global using statements ayarlandı

### 1.2 Integration Test Setup ✅
- [x] WebApplicationFactory (ECommerceWebApplicationFactory) oluşturuldu
- [x] In-Memory Database konfigurasyonu
- [x] ProductControllerIntegrationTests yazıldı

**Status:** Test altyapısı tamamen hazır ✅

---

## Phase 2: Rich Domain Model & Entity Refinement (NEXT)

### 2.1 Entity Private Setters Implementation
**Hedef:** Domain logic'i Entity'ler içinde kapsayıp, external setleme işlemini engelle

```csharp
// Domain/Entities/Product.cs - BEFORE
public class Product : AuditableEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

// Domain/Entities/Product.cs - AFTER
public class Product : AuditableEntity
{
    private Product() { } // EF Core için gerekli
    
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    
    // Factory method
    public static Product Create(string name, decimal price, int stockQuantity, int categoryId, int brandId)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name required");
        if (price <= 0) throw new ArgumentException("Price must be positive");
        if (stockQuantity < 0) throw new ArgumentException("Stock cannot be negative");
        
        return new Product
        {
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId,
            BrandId = brandId
        };
    }
    
    // Behavior methods (Domain logic)
    public void UpdateStock(int quantity)
    {
        if (quantity < 0) throw new InvalidOperationException("Stock cannot be negative");
        StockQuantity = quantity;
    }
    
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new InvalidOperationException("Price must be positive");
        Price = newPrice;
    }
    
    public void Restock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Restock quantity must be positive");
        StockQuantity += quantity;
    }
    
    public bool CanDecreaseStock(int quantity)
    {
        return StockQuantity >= quantity;
    }
}
```

**Uygulanacak Entityler:**
- [ ] Product
- [ ] Category
- [ ] Brand
- [ ] User
- [ ] Order
- [ ] Company
- [ ] Review

### 2.2 IEntityTypeConfiguration Separation

**Hedef:** OnModelCreating metodundan ayrı configuration sınıfları oluştur

```csharp
// Infrastructure/Persistence/Configurations/ProductConfiguration.cs - YENI
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(p => p.Description)
            .HasMaxLength(1000);
        
        builder.Property(p => p.Price)
            .HasPrecision(10, 2)
            .IsRequired();
        
        builder.Property(p => p.StockQuantity)
            .IsRequired();
        
        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Query filters for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

// AppDbContext.cs - UPDATED
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    base.OnModelCreating(modelBuilder);
}
```

**Oluşturulacak Configurations:**
- [ ] ProductConfiguration
- [ ] CategoryConfiguration
- [ ] BrandConfiguration
- [ ] UserConfiguration
- [ ] OrderConfiguration
- [ ] CompanyConfiguration
- [ ] ReviewConfiguration

---

## Phase 3: Code Cleanup & Constants

### 3.1 Constants/Resources Classes

```csharp
// Application/Constants/ValidationMessages.cs - YENI
namespace ECommerce.Application.Constants;

public static class ValidationMessages
{
    public const string NameRequired = "Name is required";
    public const string NameMaxLength = "Name cannot exceed 255 characters";
    public const string PriceRequired = "Price is required";
    public const string PriceMustBePositive = "Price must be greater than 0";
    public const string StockQuantityRequired = "Stock quantity is required";
    public const string StockQuantityCannotBeNegative = "Stock quantity cannot be negative";
    public const string EmailRequired = "Email is required";
    public const string EmailInvalid = "Email format is invalid";
    public const string PasswordRequired = "Password is required";
    public const string PasswordMinLength = "Password must be at least 8 characters";
}

// Application/Constants/ErrorMessages.cs - YENI
namespace ECommerce.Application.Constants;

public static class ErrorMessages
{
    public const string ProductNotFound = "Product not found";
    public const string ProductAlreadyExists = "Product with this name already exists";
    public const string CategoryNotFound = "Category not found";
    public const string BrandNotFound = "Brand not found";
    public const string UserNotFound = "User not found";
    public const string UnauthorizedAccess = "You do not have permission to perform this action";
    public const string InvalidCredentials = "Invalid credentials";
    public const string UserAlreadyExists = "User with this email already exists";
}

// Application/Constants/SuccessMessages.cs - YENI
namespace ECommerce.Application.Constants;

public static class SuccessMessages
{
    public const string ProductCreated = "Product created successfully";
    public const string ProductUpdated = "Product updated successfully";
    public const string ProductDeleted = "Product deleted successfully";
    public const string LoginSuccessful = "Login successful";
    public const string RegistrationSuccessful = "Registration successful";
}
```

**Oluşturulacak:**
- [ ] ValidationMessages.cs
- [ ] ErrorMessages.cs
- [ ] SuccessMessages.cs
- [ ] ApiRoutes.cs (Endpoint sabitler)
- [ ] AppSettings.cs (Configuration sabitler)

### 3.2 Validator Cleanup

```csharp
// Application/Features/Products/Commands/CreateProduct/CreateProductCommandValidator.cs - UPDATED
using ECommerce.Application.Constants;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(255).WithMessage(ValidationMessages.NameMaxLength);
        
        RuleFor(x => x.Request.Price)
            .NotEmpty().WithMessage(ValidationMessages.PriceRequired)
            .GreaterThan(0).WithMessage(ValidationMessages.PriceMustBePositive);
        
        RuleFor(x => x.Request.StockQuantity)
            .NotEmpty().WithMessage(ValidationMessages.StockQuantityRequired)
            .GreaterThanOrEqualTo(0).WithMessage(ValidationMessages.StockQuantityCannotBeNegative);
    }
}
```

---

## Phase 4: Additional Test Data Builders

### 4.1 CategoryTestDataBuilder

```csharp
// tests/ECommerce.Tests.Unit/Helpers/TestDataBuilders/CategoryTestDataBuilder.cs - YENI
namespace ECommerce.Tests.Unit.Helpers.TestDataBuilders;

public static class CategoryTestDataBuilder
{
    public static CreateCategoryCommandRequest CreateValidCategoryRequest(
        string name = null,
        string description = null)
    {
        return new CreateCategoryCommandRequest
        {
            Name = name ?? new Faker().Commerce.Categories(1).First(),
            Description = description ?? new Faker().Lorem.Paragraph()
        };
    }

    public static Category CreateValidCategoryEntity(
        int id = 1,
        string name = null,
        string description = null)
    {
        return Category.Create(
            name ?? new Faker().Commerce.Categories(1).First(),
            description ?? new Faker().Lorem.Paragraph()
        );
    }
}

// tests/ECommerce.Tests.Unit/Helpers/TestDataBuilders/BrandTestDataBuilder.cs - YENI
public static class BrandTestDataBuilder
{
    public static CreateBrandCommandRequest CreateValidBrandRequest(string name = null)
    {
        return new CreateBrandCommandRequest
        {
            Name = name ?? new Faker().Company.CompanyName()
        };
    }

    public static Brand CreateValidBrandEntity(int id = 1, string name = null)
    {
        return Brand.Create(name ?? new Faker().Company.CompanyName());
    }
}

// tests/ECommerce.Tests.Unit/Helpers/TestDataBuilders/UserTestDataBuilder.cs - YENI
public static class UserTestDataBuilder
{
    public static CreateUserCommandRequest CreateValidUserRequest(
        string email = null,
        string password = null,
        string username = null)
    {
        var faker = new Faker();
        return new CreateUserCommandRequest
        {
            Email = email ?? faker.Internet.Email(),
            Password = password ?? "Test@1234",
            Username = username ?? faker.Internet.UserName(),
            FullName = faker.Name.FullName(),
            PhoneNumber = faker.Phone.PhoneNumber("##########")
        };
    }

    public static User CreateValidUserEntity(
        int id = 1,
        string email = null,
        string username = null)
    {
        var faker = new Faker();
        return User.Create(
            email ?? faker.Internet.Email(),
            username ?? faker.Internet.UserName(),
            faker.Name.FullName(),
            "hashed_password"
        );
    }
}
```

---

## Phase 5: Frontend Service Layer & Typed HttpClient

### 5.1 Service Facade Pattern

```typescript
// Dashboard.Web/Services/ProductService.ts (Typescript/Angular örneği)
// - Bu C# projesinde kullanmak için ProductApiService.cs'i genişleteceğiz

// src/Application/Interfaces/IProductService.cs - C# YENI
namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
    Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync();
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductCommandRequest request);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductCommandRequest request);
    Task<ApiResponse<bool>> DeleteProductAsync(int id);
}

// src/Application/Services/ProductService.cs - YENI
namespace ECommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Failure("Product not found");

        return ApiResponse<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<ProductDto>>(products);
        return ApiResponse<IEnumerable<ProductDto>>.Success(dtos);
    }

    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductCommandRequest request)
    {
        var existingProduct = await _productRepository.AnyAsync(p => p.Name == request.Name);
        if (existingProduct)
            return ApiResponse<ProductDto>.Failure("Product already exists");

        var product = Product.Create(request.Name, request.Price, request.StockQuantity, request.CategoryId, request.BrandId);
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(int id, UpdateProductCommandRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Failure("Product not found");

        product.UpdatePrice(request.Price);
        product.UpdateStock(request.StockQuantity);

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<bool>.Failure("Product not found");

        _productRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true);
    }
}
```

### 5.2 Typed HttpClient Setup

```csharp
// src/Presentation/ECommerce.RestApi/Extensions/ServiceCollectionExtensions.cs - YENI

using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;

namespace ECommerce.RestApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }

    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5027";

        services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
        {
            client.BaseAddress = new Uri($"{apiBaseUrl}/api");
            client.DefaultRequestHeaders.Add("User-Agent", "ECommerce-Dashboard");
        })
        .ConfigureHttpClient((provider, client) =>
        {
            // Optional: Add auth token logic
        });

        services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
        {
            client.BaseAddress = new Uri($"{apiBaseUrl}/api");
            client.DefaultRequestHeaders.Add("User-Agent", "ECommerce-Dashboard");
        });

        return services;
    }
}

// Program.cs - UPDATED
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddHttpClients(builder.Configuration);
```

---

## Phase 6: Deployment & Configuration Management

### 6.1 AppSettings Configuration

```json
// src/Presentation/ECommerce.RestApi/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseSettings": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=ecommerce.db"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-here-min-32-characters-long",
    "ExpiryInMinutes": 60,
    "Issuer": "ECommerceApi",
    "Audience": "ECommerceClients"
  },
  "ApiSettings": {
    "BaseUrl": "http://localhost:5027",
    "ApiVersion": "v1"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "EnableSSL": true
  },
  "AllowedHosts": "*"
}

// src/Presentation/ECommerce.RestApi/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "DatabaseSettings": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=ecommerce-dev.db"
  }
}

// src/Presentation/ECommerce.RestApi/appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "DatabaseSettings": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=prod-server;Database=ECommerce;..."
  },
  "JwtSettings": {
    "Secret": "YOUR_PRODUCTION_SECRET_FROM_KEYVAULT",
    "ExpiryInMinutes": 60
  }
}
```

### 6.2 User Secrets Setup

```powershell
# Command Line - Çalıştırılacak
cd src/Presentation/ECommerce.RestApi
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "your-production-secret-key-here-min-32-characters"
dotnet user-secrets set "DatabaseSettings:ConnectionString" "Server=myserver;Database=mydb;..."
dotnet user-secrets set "EmailSettings:SenderPassword" "your-app-password"
```

### 6.3 Configuration Model

```csharp
// src/Application/Settings/JwtSettings.cs - YENI
namespace ECommerce.Application.Settings;

public class JwtSettings
{
    public string Secret { get; set; }
    public int ExpiryInMinutes { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

// src/Application/Settings/DatabaseSettings.cs
public class DatabaseSettings
{
    public string Provider { get; set; }
    public string ConnectionString { get; set; }
}

// src/Application/Settings/EmailSettings.cs
public class EmailSettings
{
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; }
    public bool EnableSSL { get; set; }
}

// src/Application/Settings/ApiSettings.cs
public class ApiSettings
{
    public string BaseUrl { get; set; }
    public string ApiVersion { get; set; }
}

// Program.cs Configuration
var jwtSettings = new JwtSettings();
configuration.GetSection("JwtSettings").Bind(jwtSettings);
services.AddSingleton(jwtSettings);

var databaseSettings = new DatabaseSettings();
configuration.GetSection("DatabaseSettings").Bind(databaseSettings);
services.AddSingleton(databaseSettings);
```

---

## Phase 7: GitHub Actions CI/CD Pipeline

### 7.1 Build, Test, Deploy Pipeline

```yaml
# .github/workflows/build-test-publish.yml - YENI
name: Build, Test & Publish

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

env:
  DOTNET_VERSION: '10.0.x'
  PROJECT_PATH: 'src/Presentation/ECommerce.RestApi'
  TEST_UNIT_PATH: 'tests/ECommerce.Tests.Unit'
  TEST_INTEGRATION_PATH: 'tests/ECommerce.Tests.Integration'

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
  test:
    runs-on: ubuntu-latest
    needs: build
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Run Unit Tests
      run: dotnet test ${{ env.TEST_UNIT_PATH }} --no-restore --verbosity normal
    
    - name: Run Integration Tests
      run: dotnet test ${{ env.TEST_INTEGRATION_PATH }} --no-restore --verbosity normal
    
    - name: Generate Coverage Report
      run: dotnet test --collect:"XPlat Code Coverage" --no-restore
  
  publish:
    runs-on: ubuntu-latest
    needs: test
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Publish
      run: dotnet publish ${{ env.PROJECT_PATH }} -c Release -o ${{ github.workspace }}/publish
    
    - name: Upload Artifact
      uses: actions/upload-artifact@v3
      with:
        name: app-publish
        path: ${{ github.workspace }}/publish
```

---

## Phase 8: Multi-Tenancy Improvements

### 8.1 ITenantProvider Interface Refactor

```csharp
// src/Application/Interfaces/ITenantProvider.cs
namespace ECommerce.Application.Interfaces;

public interface ITenantProvider
{
    /// <summary>
    /// Gets the current tenant's ID from the request context
    /// </summary>
    int GetCurrentTenantId();
    
    /// <summary>
    /// Gets the current tenant asynchronously with validation
    /// </summary>
    Task<Company> GetCurrentTenantAsync();
    
    /// <summary>
    /// Validates if the given tenant ID matches current context
    /// </summary>
    bool IsCurrentTenant(int tenantId);
}

// src/Infrastructure/Services/TenantProvider.cs - UPDATED
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    private const string TenantIdClaimType = "tenant_id";

    public TenantProvider(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
    }

    public int GetCurrentTenantId()
    {
        var tenantClaim = _httpContextAccessor.HttpContext?
            .User
            .FindFirst(TenantIdClaimType)?
            .Value;

        if (int.TryParse(tenantClaim, out int tenantId))
            return tenantId;

        throw new UnauthorizedAccessException("Tenant context not found");
    }

    public async Task<Company> GetCurrentTenantAsync()
    {
        var tenantId = GetCurrentTenantId();
        var tenant = await _dbContext.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == tenantId && c.IsActive);

        if (tenant == null)
            throw new UnauthorizedAccessException($"Tenant {tenantId} not found or inactive");

        return tenant;
    }

    public bool IsCurrentTenant(int tenantId)
    {
        return GetCurrentTenantId() == tenantId;
    }
}
```

---

## Phase 9: Advanced Seed Data Improvements

### 9.1 Enhanced DbSeeder with Factory Methods

```csharp
// src/Infrastructure/Persistence/Data/DbSeeder.cs - UPDATED
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Persistence.Data;

public class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedCompaniesAsync(context);
        await SeedCategoriesAsync(context);
        await SeedBrandsAsync(context);
        await SeedProductsAsync(context);
        await SeedUsersAsync(context);
        await SeedRolesAsync(context);
    }

    private static async Task SeedCompaniesAsync(AppDbContext context)
    {
        if (await context.Companies.AnyAsync())
            return;

        var companies = new List<Company>
        {
            Company.Create("TechCorp", "tech@techcorp.com", "Istanbul", "info@techcorp.com"),
            Company.Create("RetailMax", "admin@retailmax.com", "Ankara", "contact@retailmax.com"),
            Company.Create("FashionHub", "admin@fashionhub.com", "Izmir", "support@fashionhub.com")
        };

        await context.Companies.AddRangeAsync(companies);
        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            Category.Create("Electronics", "Electronic devices and accessories"),
            Category.Create("Clothing", "Men, women, and children clothing"),
            Category.Create("Books", "Physical and digital books"),
            Category.Create("Home & Garden", "Household items and garden supplies"),
            Category.Create("Sports", "Sports equipment and accessories")
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedBrandsAsync(AppDbContext context)
    {
        if (await context.Brands.AnyAsync())
            return;

        var brands = new List<Brand>
        {
            Brand.Create("Apple"),
            Brand.Create("Samsung"),
            Brand.Create("Nike"),
            Brand.Create("Adidas"),
            Brand.Create("Sony")
        };

        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(AppDbContext context)
    {
        if (await context.Products.AnyAsync())
            return;

        var electronics = await context.Categories.FirstAsync(c => c.Name == "Electronics");
        var apple = await context.Brands.FirstAsync(b => b.Name == "Apple");

        var products = new List<Product>
        {
            Product.Create("iPhone 14 Pro", 999.99m, 50, electronics.Id, apple.Id),
            Product.Create("MacBook Pro", 1999.99m, 20, electronics.Id, apple.Id),
            Product.Create("iPad Air", 599.99m, 30, electronics.Id, apple.Id)
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var company = await context.Companies.FirstAsync();
        var superAdminRole = await context.Roles.FirstAsync(r => r.Name == "SuperAdmin");

        var superAdminUser = User.Create(
            "omerkafkas55@gmail.com",
            "superadmin",
            "Omer Kafkas",
            BCrypt.Net.BCrypt.HashPassword("S5s5mr.kfks")
        );

        superAdminUser.UserRoles.Add(new UserRole { User = superAdminUser, Role = superAdminRole });

        await context.Users.AddAsync(superAdminUser);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync())
            return;

        var roles = new List<Role>
        {
            Role.Create("SuperAdmin", "System administrator with full access"),
            Role.Create("CompanyAdmin", "Company administrator"),
            Role.Create("Manager", "Department manager"),
            Role.Create("Staff", "Regular staff member"),
            Role.Create("Customer", "Regular customer")
        };

        await context.Roles.AddRangeAsync(roles);
        await context.SaveChangesAsync();
    }
}
```

---

## Implementation Checklist

### Phase 2: Domain Model
- [ ] Product entity refactored with private setters
- [ ] Category entity refactored with private setters
- [ ] Brand entity refactored with private setters
- [ ] User entity refactored with private setters
- [ ] Order entity refactored with private setters
- [ ] Company entity refactored with private setters
- [ ] Review entity refactored with private setters
- [ ] ProductConfiguration created
- [ ] CategoryConfiguration created
- [ ] BrandConfiguration created
- [ ] UserConfiguration created
- [ ] OrderConfiguration created
- [ ] CompanyConfiguration created
- [ ] ReviewConfiguration created
- [ ] AppDbContext OnModelCreating updated to use ApplyConfigurationsFromAssembly

### Phase 3: Code Cleanup
- [ ] ValidationMessages.cs created
- [ ] ErrorMessages.cs created
- [ ] SuccessMessages.cs created
- [ ] ApiRoutes.cs created
- [ ] All validators updated to use constants
- [ ] All exceptions updated to use constants
- [ ] All responses updated to use constants

### Phase 4: Test Data Builders
- [ ] CategoryTestDataBuilder created
- [ ] BrandTestDataBuilder created
- [ ] UserTestDataBuilder created
- [ ] OrderTestDataBuilder created
- [ ] CustomerTestDataBuilder created
- [ ] ReviewTestDataBuilder created

### Phase 5: Services & HttpClients
- [ ] IProductService interface created
- [ ] ProductService implementation created
- [ ] Similar services for Category, Brand, User, Order, Review
- [ ] IProductApiClient interface created
- [ ] ProductApiClient implementation created
- [ ] ServiceCollectionExtensions.AddApplicationServices created
- [ ] ServiceCollectionExtensions.AddHttpClients created

### Phase 6: Configuration
- [ ] appsettings.json created with all sections
- [ ] appsettings.Development.json created
- [ ] appsettings.Production.json created
- [ ] JwtSettings configuration class created
- [ ] DatabaseSettings configuration class created
- [ ] EmailSettings configuration class created
- [ ] ApiSettings configuration class created
- [ ] Configuration binding in Program.cs

### Phase 7: CI/CD
- [ ] .github/workflows directory created
- [ ] build-test-publish.yml created with build, test, publish jobs
- [ ] Code coverage collection configured
- [ ] Artifact upload configured

### Phase 8: Multi-Tenancy
- [ ] ITenantProvider interface updated
- [ ] TenantProvider implementation refactored
- [ ] Tenant validation added
- [ ] Tenant claim extraction improved

### Phase 9: Seed Data
- [ ] DbSeeder refactored with separate seed methods
- [ ] Company.Create factory method using seed data
- [ ] Category.Create factory method using seed data
- [ ] Brand.Create factory method using seed data
- [ ] Product.Create factory method using seed data
- [ ] User.Create factory method using seed data
- [ ] Role.Create factory method using seed data

---

## Testing Strategy

### Unit Tests
- Test all domain entity factory methods
- Test all domain entity behavior methods
- Test all validators
- Test all service methods
- Target: 80%+ code coverage

### Integration Tests
- Test all API endpoints
- Test authentication/authorization
- Test database persistence
- Test error handling
- Target: Critical paths only

### Test Data Management
- Use TestDataBuilders for consistency
- Use Bogus for realistic random data
- Isolate tests with InMemory database

---

## Architecture Principles

1. **Domain-Driven Design**
   - Rich domain models with behavior
   - Factory methods for entity creation
   - Validation in entities, not in DTOs

2. **SOLID Principles**
   - Single Responsibility: One service per entity domain
   - Open/Closed: Use interfaces for extension
   - Liskov Substitution: Services implement interfaces
   - Interface Segregation: Small focused interfaces
   - Dependency Inversion: Depend on abstractions

3. **Clean Code**
   - Meaningful names
   - Small focused methods
   - No magic strings (use Constants)
   - Proper error messages (use Resources)
   - Well-organized files

4. **Testability**
   - Dependencies injected
   - Interfaces for all services
   - No static dependencies
   - Test data builders for consistency
   - Comprehensive test coverage

---

## Deployment Considerations

1. **Environment Management**
   - Use User Secrets for local development
   - Use environment variables for Docker
   - Use Key Vault for production

2. **Database Migrations**
   - Apply migrations on application startup
   - Keep migration history clean
   - Backup before production deployments

3. **Logging & Monitoring**
   - Use structured logging (Serilog)
   - Log authentication failures
   - Log business-critical operations
   - Monitor error rates

4. **Security**
   - Validate all inputs
   - Use HTTPS in production
   - Implement CORS properly
   - Rotate secrets regularly
   - Implement rate limiting

---

## Success Metrics

- ✅ All unit tests passing
- ✅ All integration tests passing
- ✅ Code coverage > 80%
- ✅ CI/CD pipeline automated
- ✅ Zero hardcoded strings
- ✅ All entities have behavior methods
- ✅ All configurations separated
- ✅ All services follow single responsibility

---

**Last Updated:** 2024-12-13
**Status:** Implementation in Progress - Phase 1 Completed, Phase 2-9 Pending
