using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Features.Customers.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, ApiResponse<PaginatedResult<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _unitOfWork.Customers.CountAsync();
        var customers = await _unitOfWork.Customers.GetPagedAsync(request.PageNumber, request.PageSize);
        var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

        var paginatedResult = new PaginatedResult<CustomerDto>(customerDtos, totalCount, request.PageNumber, request.PageSize);

        return ApiResponse<PaginatedResult<CustomerDto>>.SuccessResponse(paginatedResult);
    }
}
