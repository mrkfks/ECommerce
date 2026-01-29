using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/login-history")]
[Authorize]
public class LoginHistoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoginHistoryController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Giriş geçmişi listesi - SuperAdmin tüm şirketleri, CompanyAdmin kendi şirketini görür
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanViewLoginHistory")]
    public async Task<IActionResult> GetAll([FromQuery] int? companyId = null, [FromQuery] int? userId = null, [FromQuery] int take = 50)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        var query = _context.LoginHistories
            .Include(lh => lh.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .OrderByDescending(lh => lh.LoginTime)
            .AsQueryable();

        // SuperAdmin değilse sadece kendi şirketinin verilerini görsün
        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            query = query.Where(lh => lh.User.CompanyId == userCompanyId.Value);
        }
        else if (companyId.HasValue)
        {
            query = query.Where(lh => lh.User.CompanyId == companyId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(lh => lh.UserId == userId.Value);
        }

        var result = await query.Take(take).Select(lh => new LoginHistoryDto
        {
            Id = lh.Id,
            UserId = lh.UserId,
            Username = lh.User.Username,
            UserFullName = $"{lh.User.FirstName} {lh.User.LastName}".Trim(),
            RoleName = lh.User.UserRoles.Select(ur => ur.RoleName).FirstOrDefault(),
            LoginTime = lh.LoginTime,
            IpAddress = lh.IpAddress,
            UserAgent = lh.UserAgent,
            Browser = lh.Browser,
            OperatingSystem = lh.OperatingSystem,
            Location = lh.Location,
            IsSuspicious = lh.IsSuspicious,
            SuspiciousReason = lh.SuspiciousReason,
            IsSuccessful = lh.IsSuccessful,
            FailureReason = lh.FailureReason,
            TimeAgo = GetTimeAgo(lh.LoginTime)
        }).ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Son 10 giriş - Dashboard widget için
    /// </summary>
    [HttpGet("recent")]
    [Authorize(Policy = "CanViewLoginHistory")]
    public async Task<IActionResult> GetRecentLogins([FromQuery] int take = 10)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        var query = _context.LoginHistories
            .Include(lh => lh.User)
                .ThenInclude(u => u.UserRoles)
            .AsNoTracking()
            .Where(lh => lh.IsSuccessful)
            .OrderByDescending(lh => lh.LoginTime)
            .AsQueryable();

        // SuperAdmin değilse sadece kendi şirketinin verilerini görsün
        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            query = query.Where(lh => lh.User.CompanyId == userCompanyId.Value);
        }

        var result = await query.Take(take).Select(lh => new LoginHistoryDto
        {
            Id = lh.Id,
            UserId = lh.UserId,
            Username = lh.User.Username,
            UserFullName = $"{lh.User.FirstName} {lh.User.LastName}".Trim(),
            RoleName = lh.User.UserRoles.Select(ur => ur.RoleName).FirstOrDefault(),
            LoginTime = lh.LoginTime,
            IpAddress = lh.IpAddress,
            Browser = lh.Browser,
            OperatingSystem = lh.OperatingSystem,
            IsSuspicious = lh.IsSuspicious,
            SuspiciousReason = lh.SuspiciousReason,
            IsSuccessful = lh.IsSuccessful,
            TimeAgo = GetTimeAgo(lh.LoginTime)
        }).ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Şüpheli girişler
    /// </summary>
    [HttpGet("suspicious")]
    [Authorize(Policy = "CanViewLoginHistory")]
    public async Task<IActionResult> GetSuspiciousLogins([FromQuery] int take = 20)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        var query = _context.LoginHistories
            .Include(lh => lh.User)
                .ThenInclude(u => u.UserRoles)
            .AsNoTracking()
            .Where(lh => lh.IsSuspicious)
            .OrderByDescending(lh => lh.LoginTime)
            .AsQueryable();

        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            query = query.Where(lh => lh.User.CompanyId == userCompanyId.Value);
        }

        var result = await query.Take(take).Select(lh => new LoginHistoryDto
        {
            Id = lh.Id,
            UserId = lh.UserId,
            Username = lh.User.Username,
            UserFullName = $"{lh.User.FirstName} {lh.User.LastName}".Trim(),
            RoleName = lh.User.UserRoles.Select(ur => ur.RoleName).FirstOrDefault(),
            LoginTime = lh.LoginTime,
            IpAddress = lh.IpAddress,
            Browser = lh.Browser,
            IsSuspicious = true,
            SuspiciousReason = lh.SuspiciousReason,
            IsSuccessful = lh.IsSuccessful,
            TimeAgo = GetTimeAgo(lh.LoginTime)
        }).ToListAsync();

        return Ok(result);
    }

    /// <summary>
    /// Giriş özeti
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Policy = "CanViewLoginHistory")]
    public async Task<IActionResult> GetSummary()
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var userCompanyId = GetCurrentUserCompanyId();

        var today = DateTime.UtcNow.Date;

        IQueryable<LoginHistory> query = _context.LoginHistories
            .Include(lh => lh.User)
            .AsNoTracking();

        if (!isSuperAdmin && userCompanyId.HasValue)
        {
            query = query.Where(lh => lh.User.CompanyId == userCompanyId.Value);
        }

        var todayLogins = await query.Where(lh => lh.LoginTime.Date == today).ToListAsync();

        var summary = new LoginHistorySummaryDto
        {
            TotalLogins = todayLogins.Count,
            SuspiciousLogins = todayLogins.Count(lh => lh.IsSuspicious),
            FailedLogins = todayLogins.Count(lh => !lh.IsSuccessful),
            UniqueUsers = todayLogins.Select(lh => lh.UserId).Distinct().Count(),
            RecentLogins = await query
                .OrderByDescending(lh => lh.LoginTime)
                .Take(10)
                .Select(lh => new LoginHistoryDto
                {
                    Id = lh.Id,
                    UserId = lh.UserId,
                    Username = lh.User.Username,
                    UserFullName = $"{lh.User.FirstName} {lh.User.LastName}".Trim(),
                    RoleName = lh.User.UserRoles.Select(ur => ur.RoleName).FirstOrDefault(),
                    LoginTime = lh.LoginTime,
                    IpAddress = lh.IpAddress,
                    IsSuspicious = lh.IsSuspicious,
                    SuspiciousReason = lh.SuspiciousReason,
                    IsSuccessful = lh.IsSuccessful,
                    TimeAgo = GetTimeAgo(lh.LoginTime)
                }).ToListAsync()
        };

        return Ok(summary);
    }

    /// <summary>
    /// Giriş kaydı oluştur (Login sırasında çağrılır) - Sadece internal kullanım için
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] LoginHistoryFormDto dto)
    {
        // Sadece kendi user ID'si ile kayıt oluşturabilir
        var currentUserId = GetCurrentUserId();
        if (currentUserId != dto.UserId && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        // Son başarılı girişi al
        var lastLogin = await _context.LoginHistories
            .Where(lh => lh.UserId == dto.UserId && lh.IsSuccessful)
            .OrderByDescending(lh => lh.LoginTime)
            .FirstOrDefaultAsync();

        // Son 1 saatteki başarısız giriş sayısı
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var failedAttempts = await _context.LoginHistories
            .CountAsync(lh => lh.UserId == dto.UserId && !lh.IsSuccessful && lh.LoginTime >= oneHourAgo);

        var loginHistory = LoginHistory.Create(
            dto.UserId,
            dto.IpAddress ?? "Unknown",
            dto.UserAgent,
            dto.Browser,
            dto.OperatingSystem,
            dto.Location,
            dto.IsSuccessful,
            dto.FailureReason);

        // Şüpheli aktivite kontrolü
        loginHistory.CheckForSuspiciousActivity(
            lastLogin?.IpAddress,
            lastLogin?.Location,
            failedAttempts);

        _context.LoginHistories.Add(loginHistory);
        await _context.SaveChangesAsync();

        return Ok(new { Id = loginHistory.Id, IsSuspicious = loginHistory.IsSuspicious });
    }

    /// <summary>
    /// Şüpheli işareti kaldır
    /// </summary>
    [HttpPost("{id}/clear-suspicion")]
    [Authorize(Policy = "CanViewLoginHistory")]
    public async Task<IActionResult> ClearSuspicion(int id)
    {
        var loginHistory = await _context.LoginHistories.FindAsync(id);
        if (loginHistory == null)
            return NotFound(new { message = "Giriş kaydı bulunamadı." });

        loginHistory.ClearSuspicion();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Şüpheli işareti kaldırıldı." });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        return 0;
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

    private static string GetTimeAgo(DateTime dateTime)
    {
        var diff = DateTime.UtcNow - dateTime;

        if (diff.TotalMinutes < 1)
            return "Az önce";
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes} dk önce";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours} saat önce";
        if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays} gün önce";

        return dateTime.ToString("dd MMM yyyy HH:mm");
    }
}
