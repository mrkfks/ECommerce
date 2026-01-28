namespace ECommerce.Application.DTOs;

/// <summary>
/// Resim yükleme sonuç DTO
/// </summary>
public record ImageUploadResultDto
{
    public string OriginalUrl { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
    public string WebPUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
}
