using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Features.Products.Queries;

public class GetAllProductsQuery : IRequest<ApiResponseDto<PaginatedResult<ProductDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
