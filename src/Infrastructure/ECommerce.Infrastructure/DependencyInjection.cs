using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Services.Notifications;
using ECommerce.Infrastructure.Services.Search;
using ECommerce.Infrastructure.Services.Caching;
using ECommerce.Infrastructure.Services.Storage;
using ECommerce.Application.Interfaces.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

        // IApplicationDbContext for direct query access
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();


        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IModelService, ModelService>();
        services.AddScoped<IGlobalAttributeService, GlobalAttributeService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IBannerService, BannerService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IFileUploadService, FileUploadService>(provider =>
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            return new FileUploadService(uploadsFolder);
        });
        
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ICompanyService, CompanyService>();
            
        // Advanced Services
        services.AddScoped<ISearchService, DatabaseSearchService>();
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>(); // New RealTime Notification Service
        services.AddScoped<IStorageService, LocalStorageService>();

        // Seeder
        services.AddScoped<DataSeeder>();

        return services;
    }
}
