using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;

        public UnitOfWork(AppDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
            Products = new ProductRepository(_context, _tenantService);
            Orders = new OrderRepository(_context);
            Customers = new CustomerRepository(_context);
            Addresses = new AddressRepository(_context);
            Companies = new CompanyRepository(_context);
            Categories = new GenericRepository<Category>(_context);
            Brands = new GenericRepository<Brand>(_context);
            Reviews = new GenericRepository<Review>(_context);
            Banners = new GenericRepository<Banner>(_context);
            ProductImages = new GenericRepository<ProductImage>(_context);
            Users = new GenericRepository<User>(_context);
            Roles = new GenericRepository<Role>(_context);
            Requests = new GenericRepository<Request>(_context);
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
        public IGenericRepository<ProductImage> ProductImages { get; }
        public IGenericRepository<User> Users { get; }
        public IGenericRepository<Role> Roles { get; }
        public IGenericRepository<Request> Requests { get; }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ECommerce.Application.Exceptions.ConcurrencyException("A concurrency conflict occurred.");
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
