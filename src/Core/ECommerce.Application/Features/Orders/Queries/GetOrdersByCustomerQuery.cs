using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries;

public class GetOrdersByCustomerQuery : IRequest<ApiResponse<PaginatedResult<OrderDto>>>
{
    public int CustomerId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
