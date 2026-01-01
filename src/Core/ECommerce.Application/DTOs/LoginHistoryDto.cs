namespace ECommerce.Application.DTOs
{
    /// <summary>
    /// Giriş geçmişi DTO
    /// </summary>
    public class LoginHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string? RoleName { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Location { get; set; }
        public bool IsSuspicious { get; set; }
        public string? SuspiciousReason { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Giriş geçmişi oluşturma DTO
    /// </summary>
    public class LoginHistoryCreateDto
    {
        public int UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Location { get; set; }
        public bool IsSuccessful { get; set; } = true;
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Son girişler özeti DTO
    /// </summary>
    public class LoginHistorySummaryDto
    {
        public int TotalLogins { get; set; }
        public int SuspiciousLogins { get; set; }
        public int FailedLogins { get; set; }
        public int UniqueUsers { get; set; }
        public List<LoginHistoryDto> RecentLogins { get; set; } = new();
    }

    /// <summary>
    /// Kullanıcı yönetimi özet DTO
    /// </summary>
    public class UserManagementSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int SuperAdminCount { get; set; }
        public int CompanyAdminCount { get; set; }
        public int CustomerCount { get; set; }
        public int TodayLogins { get; set; }
        public int SuspiciousLogins { get; set; }
        public List<UserWithRoleDto> RecentUsers { get; set; } = new();
    }

    /// <summary>
    /// Rol bilgisiyle kullanıcı DTO
    /// </summary>
    public class UserWithRoleDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole => Roles.FirstOrDefault() ?? "Customer";
        public DateTime? LastLoginTime { get; set; }
        public string? LastLoginIp { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Kullanıcı filtre DTO
    /// </summary>
    public class UserFilterDto
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Sayfalanmış kullanıcı listesi DTO
    /// </summary>
    public class PagedUserListDto
    {
        public List<UserWithRoleDto> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    /// <summary>
    /// Rol güncelleme DTO
    /// </summary>
    public class UserRoleUpdateDto
    {
        public int UserId { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    /// <summary>
    /// Kullanıcı aktivasyon DTO
    /// </summary>
    public class UserActivationDto
    {
        public int UserId { get; set; }
        public bool IsActive { get; set; }
    }
}
