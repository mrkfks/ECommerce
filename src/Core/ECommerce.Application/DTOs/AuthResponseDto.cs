namespace ECommerce.Application.DTOs;

/// <summary>
/// Kimlik doğrulama yanıt DTO
/// </summary>
public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public string? Username { get; init; }
    public List<string>? Roles { get; init; }
    public UserDto? User { get; init; }
}
