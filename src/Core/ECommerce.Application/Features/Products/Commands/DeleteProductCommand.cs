using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Commands;

public class DeleteProductCommand : IRequest<ApiResponse<bool>>
{
    public int Id { get; set; }
}
