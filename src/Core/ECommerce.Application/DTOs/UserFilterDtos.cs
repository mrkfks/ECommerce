namespace ECommerce.Application.DTOs;

/// <summary>
/// Kullanıcı yönetimi özet DTO
/// </summary>
public record UserManagementSummaryDto
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers { get; init; }
    public int SuperAdminCount { get; init; }
    public int CompanyAdminCount { get; init; }
    public int RegularUserCount { get; init; }
    public int CustomerCount { get; init; }
    public int NewUsersThisMonth { get; init; }
    public int TodayLogins { get; init; }
    public int SuspiciousLogins { get; init; }
    public List<UserWithRoleDto> RecentUsers { get; init; } = new();
}

/// <summary>
/// Rol bilgisi ile kullanıcı DTO
/// </summary>
public record UserWithRoleDto
{
    public int Id { get; init; }
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public List<string> Roles { get; init; } = new();
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime? LastLoginTime { get; init; }  // Geriye uyumluluk
    public string? LastLoginIp { get; init; }
}

/// <summary>
/// Sayfalanmış kullanıcı listesi DTO
/// </summary>
public record PagedUserListDto
{
    public List<UserWithRoleDto> Users { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int Page { get; init; }  // Geriye uyumluluk
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Kullanıcı filtreleme DTO
/// </summary>
public record UserFilterDto
{
    public string? Search { get; init; }
    public string? SearchTerm { get; init; }  // Geriye uyumluluk
    public int? CompanyId { get; init; }
    public string? Role { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Kullanıcı rol güncelleme DTO
/// </summary>
public record UserRoleUpdateDto
{
    public int UserId { get; init; }
    public List<string> Roles { get; init; } = new();
}

/// <summary>
/// Kullanıcı aktivasyon DTO
/// </summary>
public record UserActivationDto
{
    public int UserId { get; init; }
    public bool IsActive { get; init; }
}
