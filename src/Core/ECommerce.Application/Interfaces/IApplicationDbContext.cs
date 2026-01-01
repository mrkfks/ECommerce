using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Interfaces;

/// <summary>
/// DbContext abstraction for queries that need direct database access
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<User> Users { get; }
    DbSet<Company> Companies { get; }
    DbSet<Review> Reviews { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
