using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Customers.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, ApiResponse<List<CustomerDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

        return ApiResponse<List<CustomerDto>>.SuccessResponse(customerDtos);
    }
}
