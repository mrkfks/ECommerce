using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Features.Orders.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Handlers;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, ApiResponse<List<OrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(request.CustomerId);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        return ApiResponse<List<OrderDto>>.SuccessResponse(orderDtos);
    }
}
