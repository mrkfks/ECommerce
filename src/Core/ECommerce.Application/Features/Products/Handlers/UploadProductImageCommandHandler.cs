using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Features.Products.Commands;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Interfaces.Infrastructure;
using ECommerce.Application.Responses;
using ECommerce.Domain.Entities;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ECommerce.Application.Features.Products.Handlers
{
    public class UploadProductImageCommandHandler : IRequestHandler<UploadProductImageCommand, ApiResponse<ProductImageDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public UploadProductImageCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ProductImageDto>> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {request.ProductId} not found.");
            }

            // Upload Image
            var imageUrl = await _imageService.UploadImageAsync(request.FileStream, request.FileName);

            // Create Entity
            // Check if it's the first image, if so make it primary by default if not specified
            var isPrimary = request.IsPrimary;
            if (!product.Images.Any() && !isPrimary)
            {
                isPrimary = true;
            }

            var productImage = ProductImage.Create(request.ProductId, imageUrl, 0, isPrimary);
            
            // Add to collection
            product.Images.Add(productImage);
            
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<ProductImageDto>(productImage);
            return ApiResponse<ProductImageDto>.SuccessResponse(dto, "Image uploaded successfully.");
        }
    }
}
