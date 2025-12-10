using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Customers.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Customers.Handlers;

public class GetCustomerByEmailQueryHandler : IRequestHandler<GetCustomerByEmailQuery, ApiResponse<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomerByEmailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<CustomerDto>> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(request.Email);
        if (customer == null)
        {
            throw new NotFoundException("Müşteri bulunamadı");
        }

        var customerDto = _mapper.Map<CustomerDto>(customer);

        return ApiResponse<CustomerDto>.SuccessResponse(customerDto);
    }
}
