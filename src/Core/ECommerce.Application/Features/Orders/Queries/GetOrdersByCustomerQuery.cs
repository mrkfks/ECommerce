using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries;

public class GetOrdersByCustomerQuery : IRequest<ApiResponse<List<OrderDto>>>
{
    public int CustomerId { get; set; }
}
