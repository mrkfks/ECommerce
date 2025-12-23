using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new NotFoundException("Ürün bulunamadı");
        }

        product.Update(request.Product.Name, request.Product.Description, request.Product.Price, request.Product.ImageUrl);

        // IsActive durumunu güncelle
        if (request.Product.IsActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        var productDto = _mapper.Map<ProductDto>(product);

        return ApiResponse<ProductDto>.SuccessResponse(productDto, "Ürün başarıyla güncellendi");
    }
}
