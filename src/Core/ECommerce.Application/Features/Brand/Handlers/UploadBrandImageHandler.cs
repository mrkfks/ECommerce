using ECommerce.Application.Features.Brand.Commands;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Brand.Handlers
{
    public class UploadBrandImageHandler : IRequestHandler<UploadBrandImageCommand, UploadBrandImageResponse>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;

        public UploadBrandImageHandler(IFileUploadService fileUploadService, IUnitOfWork unitOfWork)
        {
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadBrandImageResponse> Handle(UploadBrandImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ImageFileBytes == null || request.ImageFileBytes.Length == 0 || string.IsNullOrEmpty(request.FileName))
                {
                    return new UploadBrandImageResponse
                    {
                        Success = false,
                        Message = "Lütfen geçerli bir dosya seçin."
                    };
                }

                var imageUrl = await _fileUploadService.UploadImageAsync(request.ImageFileBytes, request.FileName, "brands");

                var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId);
                if (brand == null)
                {
                    return new UploadBrandImageResponse
                    {
                        Success = false,
                        Message = "Marka bulunamadı."
                    };
                }

                brand.ImageUrl = imageUrl;
                brand.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Brands.Update(brand);
                await _unitOfWork.SaveChangesAsync();

                return new UploadBrandImageResponse
                {
                    Success = true,
                    Message = "Resim başarıyla yüklendi.",
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                return new UploadBrandImageResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
