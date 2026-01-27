namespace ECommerce.Application.DTOs;

/// <summary>
/// Giriş geçmişi DTO
/// </summary>
public record LoginHistoryDto(
    int Id,
    int UserId,
    string Username,
    string? UserFullName,
    string? RoleName,
    DateTime LoginTime,
    string IpAddress,
    string? UserAgent,
    string? Browser,
    string? OperatingSystem,
    string? Location,
    bool IsSuspicious,
    string? SuspiciousReason,
    bool IsSuccessful,
    string? FailureReason,
    string TimeAgo
);

/// <summary>
/// Giriş geçmişi oluşturma DTO
/// </summary>
public record LoginHistoryCreateDto(
    int UserId,
    string IpAddress,
    string? UserAgent = null,
    string? Browser = null,
    string? OperatingSystem = null,
    string? Location = null,
    bool IsSuccessful = true,
    string? FailureReason = null
);

/// <summary>
/// Son girişler özeti DTO
/// </summary>
public record LoginHistorySummaryDto(
    int TotalLogins,
    int SuspiciousLogins,
    int FailedLogins,
    int UniqueUsers,
    List<LoginHistoryDto> RecentLogins
);

/// <summary>
/// Kullanıcı yönetimi özet DTO
/// </summary>
public record UserManagementSummaryDto(
    int TotalUsers,
    int ActiveUsers,
    int InactiveUsers,
    int SuperAdminCount,
    int CompanyAdminCount,
    int CustomerCount,
    int TodayLogins,
    int SuspiciousLogins,
    List<UserWithRoleDto> RecentUsers
);

/// <summary>
/// Rol bilgisiyle kullanıcı DTO
/// </summary>
public record UserWithRoleDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    int CompanyId,
    string? CompanyName,
    List<string> Roles,
    DateTime? LastLoginTime = null,
    string? LastLoginIp = null,
    DateTime? CreatedAt = null
)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string PrimaryRole => Roles.FirstOrDefault() ?? "Customer";
}

/// <summary>
/// Kullanıcı filtre DTO
/// </summary>
public record UserFilterDto(
    string? SearchTerm = null,
    string? Role = null,
    int? CompanyId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
);

/// <summary>
/// Sayfalanmış kullanıcı listesi DTO
/// </summary>
public record PagedUserListDto(
    List<UserWithRoleDto> Users,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Rol güncelleme DTO
/// </summary>
public record UserRoleUpdateDto(
    int UserId,
    List<string> Roles
);

/// <summary>
/// Kullanıcı aktivasyon DTO
/// </summary>
public record UserActivationDto(
    int UserId,
    bool IsActive
);
