using ECommerce.Application.Interfaces;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
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

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services - yorumlandı (CQRS/MediatR kullanılıyor)
        // services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IOrderService, OrderService>();
        // services.AddScoped<ICustomerService, CustomerService>();
        // services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();
        // services.AddScoped<ICategoryService, CategoryService>();
        // services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IFileUploadService, FileUploadService>(provider =>
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            return new FileUploadService(uploadsFolder);
        });
        
        // DbSeeder
        services.AddScoped<DbSeeder>();

        return services;
    }
}
