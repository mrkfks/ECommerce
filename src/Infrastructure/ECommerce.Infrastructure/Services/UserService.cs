using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ITenantService _tenantService;

    public UserService(AppDbContext context, ITenantService tenantService)
    {
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList()
        };
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync()
    {
        var currentCompanyId = _tenantService.GetCurrentCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        IQueryable<User> query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Company);

        // SuperAdmin tüm kullanıcıları görebilir, diğerleri sadece kendi şirketlerini
        if (!isSuperAdmin)
        {
            query = query.Where(u => u.CompanyId == currentCompanyId);
        }

        var users = await query.ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            CompanyId = u.CompanyId,
            CompanyName = u.Company?.Name,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName ?? string.Empty,
            LastName = u.LastName ?? string.Empty,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList()
        }).ToList();
    }

    public async Task<IReadOnlyList<UserDto>> GetByCompanyAsync(int companyId)
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.Company)
            .Where(u => u.CompanyId == companyId)
            .ToListAsync();

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            CompanyId = u.CompanyId,
            CompanyName = u.Company?.Name,
            Username = u.Username,
            Email = u.Email,
            FirstName = u.FirstName ?? string.Empty,
            LastName = u.LastName ?? string.Empty,
            IsActive = u.IsActive,
            Roles = u.UserRoles.Select(ur => ur.Role?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList()
        }).ToList();
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? string.Empty).Where(n => !string.IsNullOrEmpty(n)).ToList()
        };
    }

    public async Task<UserDto> CreateAsync(UserCreateDto dto)
    {
        var currentCompanyId = _tenantService.GetCurrentCompanyId();
        var isSuperAdmin = _tenantService.IsSuperAdmin();

        // Güvenlik kontrolü: CompanyAdmin sadece kendi şirketine kullanıcı ekleyebilir
        if (!isSuperAdmin && dto.CompanyId.HasValue && dto.CompanyId.Value != currentCompanyId)
        {
            throw new UnauthorizedAccessException("Başka bir şirkete kullanıcı ekleme yetkiniz yok.");
        }

        // CompanyAdmin kendi şirketine kullanıcı ekler, SuperAdmin istediği şirkete ekleyebilir
        var companyId = isSuperAdmin && dto.CompanyId.HasValue && dto.CompanyId.Value > 0
            ? dto.CompanyId.Value
            : currentCompanyId;

        // Email ve username kontrolü (TÜM ŞİRKETLERDE benzersiz olmalı)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == dto.Email || u.Username == dto.Username);

        if (existingUser != null)
        {
            if (existingUser.Email == dto.Email)
                throw new InvalidOperationException("Bu email adresi zaten kullanılıyor.");
            else
                throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor.");
        }

        // Şifre hash'leme
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = User.Create(
            companyId: companyId,
            username: dto.Username,
            email: dto.Email,
            passwordHash: passwordHash,
            firstName: dto.FirstName,
            lastName: dto.LastName
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Rol atama
        if (!string.IsNullOrEmpty(dto.RoleName))
        {
            await AddRoleAsync(user.Id, dto.RoleName);
        }

        return await GetByIdAsync(user.Id) ?? throw new InvalidOperationException("Kullanıcı oluşturulduktan sonra bulunamadı.");
    }

    public async Task UpdateAsync(UserUpdateDto dto)
    {
        var user = await _context.Users.FindAsync(dto.Id);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı.");

        // Username güncellemesi için benzersizlik kontrolü (TÜM ŞİRKETLERDE)
        if (!string.IsNullOrEmpty(dto.Username) && dto.Username != user.Username)
        {
            var existingUser = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Id != dto.Id);
            if (existingUser != null)
                throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor.");
        }

        // Profil güncelleme (username dahil)
        user.UpdateProfile(dto.FirstName, dto.LastName, dto.Email, dto.Username);

        // Şirket güncelleme (SuperAdmin için)
        var isSuperAdmin = _tenantService.IsSuperAdmin();
        if (isSuperAdmin && dto.CompanyId.HasValue && dto.CompanyId.Value > 0 && dto.CompanyId.Value != user.CompanyId)
        {
            user.UpdateCompany(dto.CompanyId.Value);
        }

        // Aktiflik durumu güncelleme
        if (dto.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(int userId)
    {
        var userRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role != null ? ur.Role.Name : string.Empty)
            .Where(n => !string.IsNullOrEmpty(n))
            .ToListAsync();

        return userRoles;
    }

    public async Task AddRoleAsync(int userId, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
            throw new InvalidOperationException($"'{roleName}' rolü bulunamadı.");

        var existingUserRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (existingUserRole != null)
            return; // Zaten atanmış

        var userRole = UserRole.Create(userId, role.Id, roleName);
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveRoleAsync(int userId, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null) return;

        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

        if (userRole != null)
        {
            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SetActiveStatusAsync(int userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Kullanıcı bulunamadı.");

        if (isActive)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync();
    }
}
