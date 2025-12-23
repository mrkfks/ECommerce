using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Orders.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Orders.Handlers;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, ApiResponse<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<OrderDto>> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.Id);
        if (order == null)
        {
            throw new NotFoundException("Sipariş bulunamadı");
        }

        // Adres değişimi kontrolü (sadece Pending durumda)
        if (request.Order.AddressId.HasValue && request.Order.AddressId != order.AddressId)
        {
            if (order.Status != ECommerce.Domain.Enums.OrderStatus.Pending)
            {
                throw new BusinessException("Sadece Pending durumundaki siparişlerin adresi değiştirilebilir");
            }

            var address = await _unitOfWork.Addresses.GetByIdAsync(request.Order.AddressId.Value);
            if (address == null)
            {
                throw new NotFoundException("Adres bulunamadı");
            }
        }

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        var orderDto = _mapper.Map<OrderDto>(order);

        return ApiResponse<OrderDto>.SuccessResponse(orderDto, "Sipariş başarıyla güncellendi");
    }
}
