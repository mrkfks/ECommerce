using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data.Seed;

/// <summary>
/// Veritabanı başlangıç verilerini ekler
/// </summary>
public static class DataSeeder
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        SeedCompanies(modelBuilder);
        SeedRoles(modelBuilder);
        SeedUsers(modelBuilder);
    }

    private static void SeedCompanies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().HasData(
            new
            {
                Id = 1,
                Name = "Sistem Şirketi",
                Email = "info@system.com",
                PhoneNumber = "+90 555 000 0000",
                Address = "İstanbul, Türkiye",
                TaxNumber = "0000000000",
                IsActive = true,
                IsApproved = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );
    }

    private static void SeedRoles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new { Id = 1, Name = "SuperAdmin", Description = "Sistem yöneticisi" },
            new { Id = 2, Name = "CompanyAdmin", Description = "Şirket yöneticisi" },
            new { Id = 3, Name = "User", Description = "Standart kullanıcı" }
        );
    }

    private static void SeedUsers(ModelBuilder modelBuilder)
    {
        // SuperAdmin kullanıcı (şifre: Admin123!)
        // Hash: BCrypt ile hashlenmiş
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");

        modelBuilder.Entity<User>().HasData(
            new
            {
                Id = 1,
                Username = "admin",
                FirstName = "Super",
                LastName = "Admin",
                Email = "admin@system.com",
                PasswordHash = passwordHash,
                CompanyId = 1,
                IsActive = true,
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<UserRole>().HasData(
            new { UserId = 1, RoleId = 1, RoleName = "SuperAdmin" }
        );
    }
}
