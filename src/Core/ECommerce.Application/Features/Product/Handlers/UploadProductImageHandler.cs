using ECommerce.Application.Features.Product.Commands;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Product.Handlers
{
    public class UploadProductImageHandler : IRequestHandler<UploadProductImageCommand, UploadProductImageResponse>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;

        public UploadProductImageHandler(IFileUploadService fileUploadService, IUnitOfWork unitOfWork)
        {
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadProductImageResponse> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ImageFileBytes == null || request.ImageFileBytes.Length == 0 || string.IsNullOrEmpty(request.FileName))
                {
                    return new UploadProductImageResponse
                    {
                        Success = false,
                        Message = "Lütfen geçerli bir dosya seçin."
                    };
                }

                var imageUrl = await _fileUploadService.UploadImageAsync(request.ImageFileBytes, request.FileName, "products");

                var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
                if (product == null)
                {
                    return new UploadProductImageResponse
                    {
                        Success = false,
                        Message = "Ürün bulunamadı."
                    };
                }

                product.ImageUrl = imageUrl;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveChangesAsync();

                return new UploadProductImageResponse
                {
                    Success = true,
                    Message = "Resim başarıyla yüklendi.",
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                return new UploadProductImageResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
