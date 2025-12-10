using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public class UpdateProductCommand : IRequest<ApiResponse<ProductDto>>
{
    public int Id { get; set; }
    public ProductUpdateDto Product { get; set; } = null!;
}
