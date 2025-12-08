using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        ICustomerRepository Customers { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Brand> Brands { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }

        Task<int> SaveChangesAsync();
    }
}