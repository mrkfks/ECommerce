using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Commands;

public class DeleteCustomerCommand : IRequest<ApiResponse<bool>>
{
    public int Id { get; set; }
}
