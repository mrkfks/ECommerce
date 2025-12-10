using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries;

public class GetProductsByCategoryQuery : IRequest<ApiResponse<List<ProductDto>>>
{
    public int CategoryId { get; set; }
}
