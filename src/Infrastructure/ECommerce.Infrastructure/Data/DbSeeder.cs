using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;

    public DbSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            Console.WriteLine("🔄 DbSeeder.SeedAsync() başlıyor...");
            
            // Veritabanını oluştur
            Console.WriteLine("  - Migrations uygulanıyor...");
            await _context.Database.MigrateAsync();
            Console.WriteLine("  ✅ Migrations tamamlandı.");

            // Rolleri önce oluştur
            Console.WriteLine("  - Roller oluşturuluyor...");
            try
            {
                await SeedRolesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ SeedRolesAsync error: {ex.Message}");
            }

            // Demo Companies Ekle
            Console.WriteLine("  - Demo şirketleri oluşturuluyor...");
            try
            {
                await SeedDemoCompaniesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ SeedDemoCompaniesAsync error: {ex.Message}");
            }

            // SuperAdmin User Ekle
            Console.WriteLine("  - SuperAdmin kullanıcısı oluşturuluyor...");
            try
            {
                await SeedSuperAdminAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ SeedSuperAdminAsync error: {ex.Message}");
            }

            // Demo Company Users Ekle
            Console.WriteLine("  - Demo şirket kullanıcıları oluşturuluyor...");
            try
            {
                await SeedDemoCompanyUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ SeedDemoCompanyUsersAsync error: {ex.Message}");
            }

            Console.WriteLine("✅ Veritabanı başarıyla hazırlandı!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Seeding error in SeedAsync: {ex.GetType().Name}");
            Console.WriteLine($"❌ Error message: {ex.Message}");
            Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            Console.WriteLine("⚠️  Continuing application despite seeding errors...");
        }
    }

    private async Task SeedRolesAsync()
    {
        try
        {
            Console.WriteLine("    [SeedRolesAsync] başlanıyor...");
            var roles = new[] { "SuperAdmin", "CompanyAdmin", "User" };

            foreach (var roleName in roles)
            {
                Console.WriteLine($"    [SeedRolesAsync] '{roleName}' rolü kontrol ediliyor...");
                var existingRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName);

                if (existingRole == null)
                {
                    Console.WriteLine($"    [SeedRolesAsync] '{roleName}' rolü oluşturuluyor...");
                    var role = Role.Create(roleName);
                    _context.Roles.Add(role);
                    Console.WriteLine($"    [SeedRolesAsync] '{roleName}' rolü eklendi.");
                }
                else
                {
                    Console.WriteLine($"    [SeedRolesAsync] '{roleName}' rolü zaten mevcut.");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Roller oluşturuldu.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SeedRolesAsync error: {ex.Message}");
            Console.WriteLine($"❌ SeedRolesAsync stack: {ex.StackTrace}");
            // Don't throw, just log and continue
            Console.WriteLine("⚠️  Continuing despite role seeding error...");
        }
    }

    private async Task SeedSuperAdminAsync()
    {
        // Bu metod artık SeedDemoCompanyUsersAsync içinde yapılıyor
        await Task.CompletedTask;
    }

    private async Task SeedDemoCompaniesAsync()
    {
        var companies = new[]
        {
            new { Name = "mrkfks", Email = "omerkafkas55@gmail.com", Phone = "+905300839355", TaxNumber = "17636734636", Address = "Durali Alıç Mahallesi Şehit Hakan Yorulmaz Caddesi No:51 Kat:10 Daire:35 Mamak/Ankara" },
            new { Name = "alican", Email = "alican@company.com", Phone = "+905001234567", TaxNumber = "11111111111", Address = "İstanbul, Türkiye" },
            new { Name = "velican", Email = "velican@company.com", Phone = "+905007654321", TaxNumber = "22222222222", Address = "Ankara, Türkiye" }
        };

        foreach (var companyData in companies)
        {
            var existingCompany = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == companyData.Name);

            if (existingCompany == null)
            {
                var company = Company.Create(
                    name: companyData.Name,
                    address: companyData.Address,
                    phoneNumber: companyData.Phone,
                    email: companyData.Email,
                    taxNumber: companyData.TaxNumber
                );

                // Tüm demo şirketleri otomatik onaylı olsun
                company.Approve();

                _context.Companies.Add(company);
                Console.WriteLine($"  ✅ '{companyData.Name}' şirketi oluşturuldu.");
            }
            else
            {
                // Varsa onaylı yap
                if (!existingCompany.IsApproved)
                {
                    existingCompany.Approve();
                }
                Console.WriteLine($"  ℹ️  '{companyData.Name}' şirketi zaten mevcut.");
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedDemoCompanyUsersAsync()
    {
        var companyUsers = new[]
        {
            new { CompanyName = "mrkfks", Username = "mrkfks", Email = "omerkafkas55@gmail.com", Password = "S5s5mr.kfks", FirstName = "Ömer", LastName = "Kafkas", Role = "SuperAdmin" },
            new { CompanyName = "alican", Username = "alican", Email = "alican@company.com", Password = "Alican123!", FirstName = "Ali", LastName = "Can", Role = "CompanyAdmin" },
            new { CompanyName = "velican", Username = "velican", Email = "velican@company.com", Password = "Velican123!", FirstName = "Veli", LastName = "Can", Role = "CompanyAdmin" }
        };

        foreach (var userData in companyUsers)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Name == userData.CompanyName);

            if (company == null)
            {
                Console.WriteLine($"  ❌ '{userData.CompanyName}' şirketi bulunamadı, kullanıcı oluşturulamadı.");
                continue;
            }

            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == userData.Email);

            if (existingUser == null)
            {
                var user = User.Create(
                    companyId: company.Id,
                    username: userData.Username,
                    email: userData.Email,
                    passwordHash: BCrypt.Net.BCrypt.HashPassword(userData.Password),
                    firstName: userData.FirstName,
                    lastName: userData.LastName
                );

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Rol ata
                var role = await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == userData.Role);

                if (role != null)
                {
                    var userRole = UserRole.Create(user.Id, role.Id, userData.Role);
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                Console.WriteLine($"  ✅ '{userData.Username}' kullanıcısı '{userData.CompanyName}' şirketine eklendi.");
                Console.WriteLine($"     Email: {userData.Email}, Password: {userData.Password}");
            }
            else
            {
                Console.WriteLine($"  ℹ️  '{userData.Username}' kullanıcısı zaten mevcut.");
            }
        }
    }

    private async Task SeedDemoCompanyAsync()
    {
        // Bu metod artık kullanılmıyor, SeedDemoCompaniesAsync kullanılıyor
        await Task.CompletedTask;
    }
}

