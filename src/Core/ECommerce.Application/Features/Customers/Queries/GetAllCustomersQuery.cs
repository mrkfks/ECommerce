using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Queries;

public class GetAllCustomersQuery : IRequest<ApiResponse<PaginatedResult<CustomerDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
