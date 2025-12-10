using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<ApiResponse<List<ProductDto>>>
{
}
