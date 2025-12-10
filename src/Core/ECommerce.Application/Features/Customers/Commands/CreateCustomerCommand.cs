using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Commands;

public class CreateCustomerCommand : IRequest<ApiResponse<CustomerDto>>
{
    public CustomerCreateDto Customer { get; set; } = null!;
}
