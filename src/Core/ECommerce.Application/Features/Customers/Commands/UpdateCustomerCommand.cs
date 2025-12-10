using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Commands;

public class UpdateCustomerCommand : IRequest<ApiResponse<CustomerDto>>
{
    public int Id { get; set; }
    public CustomerUpdateDto Customer { get; set; } = null!;
}
