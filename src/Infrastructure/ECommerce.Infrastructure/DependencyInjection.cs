using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

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
        services.AddScoped<ICustomerMessageService, CustomerMessageService>();
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
        services.AddScoped<IPaymentService, FakePaymentService>();

        // Elasticsearch DI
        services.AddSingleton<IElasticClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            // Öncelik: Environment variable, sonra appsettings
            var uri =
                Environment.GetEnvironmentVariable("ELASTICSEARCH_URI") ??
                config["Elasticsearch:Uri"] ??
                "http://localhost:9200";
            var defaultIndex =
                Environment.GetEnvironmentVariable("ELASTICSEARCH_INDEX") ??
                config["Elasticsearch:DefaultIndex"] ??
                "products";
            var settings = new Nest.ConnectionSettings(new Uri(uri)).DefaultIndex(defaultIndex);
            return new Nest.ElasticClient(settings);
        });
        services.AddScoped<ISearchService, ElasticsearchProductSearchService>();
        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddScoped<IRealTimeNotificationService, SignalRNotificationService>(); // New RealTime Notification Service
        services.AddScoped<IStorageService, LocalStorageService>();

        // Seeder
        services.AddScoped<DataSeeder>();

        return services;
    }
}
