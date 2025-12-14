using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
            Customers = new CustomerRepository(_context);
            Addresses = new AddressRepository(_context);
            Companies = new CompanyRepository(_context);
            Categories = new GenericRepository<Category>(_context);
            Brands = new GenericRepository<Brand>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Banners = new GenericRepository<Banner>(_context);
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
        }

        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public ICustomerRepository Customers { get; }
        public IAddressRepository Addresses { get; }
        public ICompanyRepository Companies { get; }
        public IGenericRepository<Category> Categories { get; }
        public IGenericRepository<Brand> Brands { get; }
        public IGenericRepository<Review> Reviews { get; }
        public IGenericRepository<Banner> Banners { get; }
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Role> Roles { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
