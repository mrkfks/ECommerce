using ECommerce.Domain.Entities;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.UnitsOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(        
            AppDbContext context,
            IProductRepository products,
            IOrderRepository orders,
            ICustomerRepository customers,
            IRewiewRepository reviews,
            IBrandRepository brands,
            ICategoryRepository categories,
            IUserRepository users,
            IRoleRepository roles,
            IAdressRepository adress,
            IBannerRepository banners)
        {
            _context = context;

            Products = products;
            Orders = orders;
            Customers = customers;
            Reviews = reviews;
            Brands = brands;
            Categories = categories;
            Users = users;
            Roles = roles;
            Address = adress;
            Banners = banners;
        }

        public IProductRepository Products { get; }
        public IOrderRepository Orders { get; }
        public ICustomerRepository Customers { get; }
        public IRewiewRepository Reviews { get; }
        public IBrandRepository Brands { get; }
        public ICategoryRepository Categories { get; }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IAdressRepository Address { get; }
        public IBannerRepository Banners { get; }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
        
    }
}