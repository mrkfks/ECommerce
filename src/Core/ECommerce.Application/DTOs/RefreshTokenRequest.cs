namespace ECommerce.Application.DTOs;

/// <summary>
/// Token yenileme isteği DTO
/// </summary>
public record RefreshTokenRequest
{
    public string Token { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
