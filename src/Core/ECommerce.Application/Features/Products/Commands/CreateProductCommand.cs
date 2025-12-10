using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public class CreateProductCommand : IRequest<ApiResponse<ProductDto>>
{
    public ProductCreateDto Product { get; set; } = null!;
}
