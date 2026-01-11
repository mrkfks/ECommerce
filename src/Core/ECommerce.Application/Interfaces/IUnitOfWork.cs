using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IAddressRepository Addresses { get; }
    ICompanyRepository Companies { get; }
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<Brand> Brands { get; }
    IGenericRepository<Review> Reviews { get; }
    IGenericRepository<Banner> Banners { get; }
    IGenericRepository<ProductImage> ProductImages { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<Request> Requests { get; }

    Task<int> SaveChangesAsync();
}
