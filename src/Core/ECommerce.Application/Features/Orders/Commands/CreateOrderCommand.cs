using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands;

public class CreateOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public OrderCreateDto Order { get; set; } = null!;
}
