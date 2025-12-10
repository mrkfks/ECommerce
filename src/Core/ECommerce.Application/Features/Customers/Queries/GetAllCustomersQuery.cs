using ECommerce.Application.DTOs;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Queries;

public class GetAllCustomersQuery : IRequest<ApiResponse<List<CustomerDto>>>
{
}
