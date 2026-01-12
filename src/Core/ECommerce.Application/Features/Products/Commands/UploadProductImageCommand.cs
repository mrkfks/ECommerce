using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;
using System.IO;

namespace ECommerce.Application.Features.Products.Commands
{
    public class UploadProductImageCommand : IRequest<ApiResponse<ProductImageDto>>
    {
        public int ProductId { get; set; }
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
