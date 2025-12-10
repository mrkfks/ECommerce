using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands;

public class UpdateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public int Id { get; set; }
    public OrderUpdateDto Order { get; set; } = null!;
}
