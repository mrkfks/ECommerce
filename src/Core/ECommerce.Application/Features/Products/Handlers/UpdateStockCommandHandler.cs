using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            throw new NotFoundException("Ürün bulunamadı");
        }

        var newStock = product.StockQuantity + request.Quantity;
        if (newStock < 0)
        {
            throw new BusinessException("Stok miktarı negatif olamaz");
        }

        product.StockQuantity = newStock;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        try 
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (ECommerce.Application.Exceptions.ConcurrencyException)
        {
            throw new BusinessException("Stok bilgisi başka bir işlem tarafından değiştirildi. Lütfen sayfayı yenileyip tekrar deneyin.");
        }

        return ApiResponse<bool>.SuccessResponse(true, "Stok başarıyla güncellendi");
    }
}
