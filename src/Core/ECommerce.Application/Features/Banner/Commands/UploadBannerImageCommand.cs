using MediatR;

namespace ECommerce.Application.Features.Banner.Commands
{
    public class UploadBannerImageCommand : IRequest<UploadBannerImageResponse>
    {
        public int BannerId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string? FileName { get; set; }
    }

    public class UploadBannerImageResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
    }
}
