using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Orders.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using ECommerce.Domain.Enums;
using MediatR;

namespace ECommerce.Application.Features.Orders.Handlers;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.Id);
        if (order == null)
        {
            throw new NotFoundException("Sipariş bulunamadı");
        }

        if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Cancelled)
        {
            throw new BusinessException("İptal edilmiş veya teslim edilmiş sipariş iptal edilemez");
        }

        // Stokları geri ekle
        foreach (var item in order.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity += item.Quantity;
                _unitOfWork.Products.Update(product);
            }
        }

        order.Status = OrderStatus.Cancelled;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Sipariş başarıyla iptal edildi");
    }
}
