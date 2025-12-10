using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new NotFoundException("Ürün bulunamadı");
        }

        _unitOfWork.Products.Delete(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Ürün başarıyla silindi");
    }
}
