using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Features.Orders.Queries;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Handlers;

public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, ApiResponse<PaginatedResult<OrderDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PaginatedResult<OrderDto>>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        // For count, ideally repository method. Using quick count for now.
        // Assuming GenericRepository CountAsync() counts ALL. We need ByCustomer.
        // But let's assume filtering logic is handled or we need to add it.
        // Actually, to correctly count filtered items, we should use a specification or custom query.
        // Since I want to pass build first, I'll use generic CountAsync() (WRONG LOGIC but compiles) 
        // OR better: use dbset count.
        // _unitOfWork.Orders.FindAsync(x => x.CustomerId == request.CustomerId).Result.Count
        
        // Let's implement correct logic later, prioritizing build.
        // Using FindAsync logic which matches current implementation:
        var allOrdersForCustomer = await _unitOfWork.Orders.FindAsync(o => o.CustomerId == request.CustomerId);
        var totalCount = allOrdersForCustomer.Count;

        var orders = await _unitOfWork.Orders.GetOrdersByCustomerAsync(request.CustomerId, request.PageNumber, request.PageSize);
        var orderDtos = _mapper.Map<List<OrderDto>>(orders);

        var paginatedResult = new PaginatedResult<OrderDto>(orderDtos, totalCount, request.PageNumber, request.PageSize);

        return ApiResponse<PaginatedResult<OrderDto>>.SuccessResponse(paginatedResult);
    }
}
