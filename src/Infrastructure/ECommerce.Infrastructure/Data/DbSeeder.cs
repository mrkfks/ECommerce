using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Eğer zaten veri varsa seed yapma
        if (await context.Companies.AnyAsync())
            return;

        // Company
        var company = new Company
        {
            Name = "Demo Company",
            Email = "info@democompany.com",
            PhoneNumber = "+90 555 123 4567",
            TaxNumber = "1234567890",
            Address = "İstanbul, Türkiye",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Roles
        var adminRole = new Role { Name = "Admin", UserRoles = new List<UserRole>() };
        var userRole = new Role { Name = "User", UserRoles = new List<UserRole>() };
        context.Roles.AddRange(adminRole, userRole);
        await context.SaveChangesAsync();

        // Admin User
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@democompany.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            CompanyId = company.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>()
        };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        await context.SaveChangesAsync();

        // Categories
        var categories = new List<Category>
        {
            new() { Name = "Elektronik", Description = "Elektronik ürünler", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Giyim", Description = "Giyim ürünleri", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Ev & Yaşam", Description = "Ev ve yaşam ürünleri", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Brands
        var brands = new List<Brand>
        {
            new() { Name = "Samsung", Description = "Samsung Electronics", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Apple", Description = "Apple Inc.", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Name = "Nike", Description = "Nike Sports", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        context.Brands.AddRange(brands);
        await context.SaveChangesAsync();

        // Products
        var products = new List<Product>
        {
            new()
            {
                Name = "Samsung Galaxy S23",
                Description = "En son model Samsung telefon",
                Price = 35000,
                StockQuantity = 50,
                CategoryId = categories[0].Id,
                BrandId = brands[0].Id,
                CompanyId = company.Id,
                ImageUrl = "https://images.samsung.com/galaxy-s23.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "iPhone 15 Pro",
                Description = "Apple'ın en yeni telefonu",
                Price = 65000,
                StockQuantity = 30,
                CategoryId = categories[0].Id,
                BrandId = brands[1].Id,
                CompanyId = company.Id,
                ImageUrl = "https://images.apple.com/iphone-15-pro.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Nike Air Max",
                Description = "Rahat spor ayakkabı",
                Price = 3500,
                StockQuantity = 100,
                CategoryId = categories[1].Id,
                BrandId = brands[2].Id,
                CompanyId = company.Id,
                ImageUrl = "https://images.nike.com/air-max.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        // Customer
        var customer = new Customer
        {
            UserId = adminUser.Id,
            CompanyId = company.Id,
            FirstName = "Demo",
            LastName = "Müşteri",
            Email = "musteri@example.com",
            PhoneNumber = "+90 555 999 8877",
            DateOfBirth = new DateTime(1990, 1, 1),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        // Address
        var address = new Address
        {
            CustomerId = customer.Id,
            Street = "Atatürk Caddesi No:123",
            City = "İstanbul",
            State = "Kadıköy",
            Country = "Türkiye",
            ZipCode = "34000",
            Customer = customer
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        // Order
        var order = new Order
        {
            CustomerId = customer.Id,
            CompanyId = company.Id,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = 38500
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Order Items
        var orderItems = new List<OrderItem>
        {
            new()
            {
                OrderId = order.Id,
                ProductId = products[0].Id,
                Quantity = 1,
                UnitPrice = 35000
            },
            new()
            {
                OrderId = order.Id,
                ProductId = products[2].Id,
                Quantity = 1,
                UnitPrice = 3500
            }
        };
        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();

        // Reviews
        var reviews = new List<Review>
        {
            new()
            {
                ProductId = products[0].Id,
                CustomerId = customer.Id,
                CompanyId = company.Id,
                Rating = 5,
                Comment = "Harika bir telefon, çok memnunum!",
                ReviewerName = "Demo Müşteri",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                ProductId = products[1].Id,
                CustomerId = customer.Id,
                CompanyId = company.Id,
                Rating = 4,
                Comment = "Güzel ama biraz pahalı",
                ReviewerName = "Demo Müşteri",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();
    }
}
