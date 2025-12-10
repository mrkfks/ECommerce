using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Queries;

public class GetOrderByIdQuery : IRequest<ApiResponse<OrderDto>>
{
    public int Id { get; set; }
}
