using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries;

public class GetProductByIdQuery : IRequest<ApiResponse<ProductDto>>
{
    public int Id { get; set; }
}
