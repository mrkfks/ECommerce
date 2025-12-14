using MediatR;

namespace ECommerce.Application.Features.Brand.Commands
{
    public class UploadBrandImageCommand : IRequest<UploadBrandImageResponse>
    {
        public int BrandId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string? FileName { get; set; }
    }

    public class UploadBrandImageResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
    }
}
