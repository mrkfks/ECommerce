using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Queries;

public class SearchProductsQuery : IRequest<ApiResponse<List<ProductDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
}
