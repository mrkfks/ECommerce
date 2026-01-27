namespace ECommerce.Application.DTOs;

public record ImageUploadResultDto(
    string OriginalUrl,
    string WebPUrl,
    string ThumbnailUrl
);
