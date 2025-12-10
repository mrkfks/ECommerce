using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public class UpdateStockCommand : IRequest<ApiResponse<bool>>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
