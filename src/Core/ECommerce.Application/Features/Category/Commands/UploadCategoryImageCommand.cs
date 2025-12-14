using MediatR;

namespace ECommerce.Application.Features.Category.Commands
{
    public class UploadCategoryImageCommand : IRequest<UploadCategoryImageResponse>
    {
        public int CategoryId { get; set; }
        public byte[]? ImageFileBytes { get; set; }
        public string? FileName { get; set; }
    }

    public class UploadCategoryImageResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ImageUrl { get; set; }
    }
}
