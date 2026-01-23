namespace ECommerce.Application.DTOs;

public class ImageUploadResultDto
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string WebPUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
}
