using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageUsers")]
public class UserManagementController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserManagementController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kullanıcı yönetimi özeti
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();
        var today = DateTime.UtcNow.Date;

        IQueryable<User> userQuery = _context.Users
            .Include(u => u.UserRoles)
            .AsNoTracking();

        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            userQuery = userQuery.Where(u => u.CompanyId == userCompanyId.Value);
        }

        var users = await userQuery.ToListAsync();

        var todayLogins = await _context.LoginHistories
            .Where(lh => lh.LoginTime.Date == today && lh.IsSuccessful)
            .Select(lh => lh.UserId)
            .Distinct()
            .CountAsync();

        var suspiciousLogins = await _context.LoginHistories
            .Where(lh => lh.IsSuspicious && lh.LoginTime.Date == today)
            .CountAsync();

        var summary = new UserManagementSummaryDto
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive),
            InactiveUsers = users.Count(u => !u.IsActive),
            SuperAdminCount = users.Count(u => u.UserRoles.Any(ur => ur.RoleName == "SuperAdmin")),
            CompanyAdminCount = users.Count(u => u.UserRoles.Any(ur => ur.RoleName == "CompanyAdmin")),
            CustomerCount = users.Count(u => u.UserRoles.Any(ur => ur.RoleName == "Customer")),
            TodayLogins = todayLogins,
            SuspiciousLogins = suspiciousLogins,
            RecentUsers = users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new UserWithRoleDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    CompanyId = u.CompanyId,
                    Roles = u.UserRoles.Select(ur => ur.RoleName).ToList(),
                    CreatedAt = u.CreatedAt
                }).ToList()
        };

        return Ok(summary);
    }

    /// <summary>
    /// Filtrelenmiş ve sayfalanmış kullanıcı listesi
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserFilterDto filter)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        IQueryable<User> query = _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.Company)
            .AsNoTracking();

        // SuperAdmin değilse sadece kendi şirketinin kullanıcılarını görsün
        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            query = query.Where(u => u.CompanyId == userCompanyId.Value);
        }
        else if (filter.CompanyId.HasValue)
        {
            query = query.Where(u => u.CompanyId == filter.CompanyId.Value);
        }

        // Arama
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(term)));
        }

        // Rol filtresi
        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.RoleName == filter.Role));
        }

        // Aktiflik filtresi
        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Son giriş bilgilerini al
        var userIds = users.Select(u => u.Id).ToList();
        var lastLogins = await _context.LoginHistories
            .Where(lh => userIds.Contains(lh.UserId) && lh.IsSuccessful)
            .GroupBy(lh => lh.UserId)
            .Select(g => new { UserId = g.Key, LastLogin = g.Max(lh => lh.LoginTime), LastIp = g.OrderByDescending(lh => lh.LoginTime).First().IpAddress })
            .ToListAsync();

        var result = new PagedUserListDto
        {
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            Users = users.Select(u =>
            {
                var lastLogin = lastLogins.FirstOrDefault(ll => ll.UserId == u.Id);
                return new UserWithRoleDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    CompanyId = u.CompanyId,
                    CompanyName = u.Company?.Name,
                    Roles = u.UserRoles.Select(ur => ur.RoleName).ToList(),
                    LastLoginTime = lastLogin?.LastLogin,
                    LastLoginIp = lastLogin?.LastIp,
                    CreatedAt = u.CreatedAt
                };
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Tek kullanıcı detayı
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        // Yetki kontrolü
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        if (!isSuperAdmin && userCompanyId.HasValue && user.CompanyId != userCompanyId.Value)
        {
            return Forbid();
        }

        var lastLogin = await _context.LoginHistories
            .Where(lh => lh.UserId == id && lh.IsSuccessful)
            .OrderByDescending(lh => lh.LoginTime)
            .FirstOrDefaultAsync();

        var result = new UserWithRoleDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CompanyId = user.CompanyId,
            CompanyName = user.Company?.Name,
            Roles = user.UserRoles.Select(ur => ur.RoleName).ToList(),
            LastLoginTime = lastLogin?.LoginTime,
            LastLoginIp = lastLogin?.IpAddress,
            CreatedAt = user.CreatedAt
        };

        return Ok(result);
    }

    /// <summary>
    /// Kullanıcı rollerini güncelle
    /// </summary>
    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(int id, [FromBody] UserRoleUpdateDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        // Yetki kontrolü
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        if (!isSuperAdmin && userCompanyId.HasValue && user.CompanyId != userCompanyId.Value)
        {
            return Forbid();
        }

        // SuperAdmin rolü sadece SuperAdmin tarafından atanabilir
        if (dto.Roles.Contains("SuperAdmin") && !isSuperAdmin)
        {
            return BadRequest(new { message = "SuperAdmin rolü sadece SuperAdmin tarafından atanabilir." });
        }

        // Mevcut rolleri sil
        var existingRoles = user.UserRoles.ToList();
        foreach (var role in existingRoles)
        {
            _context.UserRoles.Remove(role);
        }

        // Yeni rolleri ekle
        foreach (var roleName in dto.Roles)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                var userRole = UserRole.Create(user.Id, role.Id, role.Name);
                _context.UserRoles.Add(userRole);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Roller güncellendi." });
    }

    /// <summary>
    /// Kullanıcı aktivasyon durumunu değiştir
    /// </summary>
    [HttpPut("{id}/activation")]
    public async Task<IActionResult> UpdateActivation(int id, [FromBody] UserActivationDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        // Yetki kontrolü
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        if (!isSuperAdmin && userCompanyId.HasValue && user.CompanyId != userCompanyId.Value)
        {
            return Forbid();
        }

        if (dto.IsActive)
            user.Activate();
        else
            user.Deactivate();

        await _context.SaveChangesAsync();
        return Ok(new { message = dto.IsActive ? "Kullanıcı aktifleştirildi." : "Kullanıcı devre dışı bırakıldı." });
    }

    /// <summary>
    /// Rol listesi
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");

        var roles = await _context.Roles.AsNoTracking().ToListAsync();

        // CompanyAdmin, SuperAdmin rolünü göremez
        if (!isSuperAdmin)
        {
            roles = roles.Where(r => r.Name != "SuperAdmin").ToList();
        }

        return Ok(roles.Select(r => new { r.Id, r.Name, r.Description }));
    }

    private int? GetCurrentUserCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId");
        if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
        {
            return companyId;
        }
        return null;
    }
}
