using MediatR;

namespace ECommerce.Application.Features.Product.Commands
{
    public class UploadProductImageCommand : IRequest<UploadProductImageResponse>
    {
        public int ProductId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string? FileName { get; set; }
    }

    public class UploadProductImageResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
    }
}
