namespace ECommerce.Application.DTOs;

/// <summary>
/// Giriş isteği DTO
/// </summary>
public record LoginDto
{
    public string LoginIdentifier { get; init; } = string.Empty; // Email veya Username
    public string UsernameOrEmail { get; init; } = string.Empty; // Geriye uyumluluk için
    public string Password { get; init; } = string.Empty;
}
