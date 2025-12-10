using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Commands;

public class CancelOrderCommand : IRequest<ApiResponse<bool>>
{
    public int Id { get; set; }
}
