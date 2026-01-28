namespace ECommerce.Application.DTOs;

/// <summary>
/// Giriş geçmişi DTO
/// </summary>
public record LoginHistoryDto
{
    public int Id { get; init; }
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string UserFullName { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Browser { get; init; }
    public string? OperatingSystem { get; init; }
    public string? Location { get; init; }
    public bool IsSuccess { get; init; }
    public bool IsSuccessful { get; init; }  // Geriye uyumluluk
    public string? FailureReason { get; init; }
    public bool IsSuspicious { get; init; }
    public string? SuspiciousReason { get; init; }
    public DateTime LoginTime { get; init; }
    public string? TimeAgo { get; init; }
}

/// <summary>
/// Giriş geçmişi oluşturma DTO
/// </summary>
public record LoginHistoryCreateDto
{
    public int UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Browser { get; init; }
    public string? OperatingSystem { get; init; }
    public string? Location { get; init; }
    public bool IsSuccess { get; init; } = true;
    public bool IsSuccessful { get; init; } = true;  // Geriye uyumluluk
    public string? FailureReason { get; init; }
}

/// <summary>
/// Giriş geçmişi özeti DTO
/// </summary>
public record LoginHistorySummaryDto
{
    public int TotalLogins { get; init; }
    public int SuccessfulLogins { get; init; }
    public int FailedLogins { get; init; }
    public int SuspiciousLogins { get; init; }
    public int UniqueUsers { get; init; }
    public List<LoginHistoryDto> RecentLogins { get; init; } = new();
}
