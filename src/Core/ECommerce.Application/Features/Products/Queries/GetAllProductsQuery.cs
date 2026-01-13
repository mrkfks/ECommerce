using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<ApiResponse<PaginatedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
