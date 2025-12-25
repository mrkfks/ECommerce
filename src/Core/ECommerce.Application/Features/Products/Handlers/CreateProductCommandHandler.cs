using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Responses;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Features.Products.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = ECommerce.Domain.Entities.Product.Create(
            request.Product.Name,
            request.Product.Description,
            request.Product.Price,
            request.Product.CategoryId,
            request.Product.BrandId,
            request.Product.CompanyId,
            request.Product.StockQuantity,
            null, // modelId - opsiyonel
            request.Product.ImageUrl,
            null  // sku - opsiyonel
        );

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        var productDto = _mapper.Map<ProductDto>(product);

        return ApiResponse<ProductDto>.SuccessResponse(productDto, "Ürün başarıyla oluşturuldu");
    }
}
