using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Queries;

public class GetCustomerByIdQuery : IRequest<ApiResponse<CustomerDto>>
{
    public int Id { get; set; }
}
