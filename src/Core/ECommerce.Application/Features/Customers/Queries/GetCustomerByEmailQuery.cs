using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Queries;

public class GetCustomerByEmailQuery : IRequest<ApiResponse<CustomerDto>>
{
    public string Email { get; set; } = string.Empty;
}
