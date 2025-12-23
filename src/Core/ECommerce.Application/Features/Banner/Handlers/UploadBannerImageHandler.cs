using ECommerce.Application.Features.Banner.Commands;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Banner.Handlers
{
    public class UploadBannerImageHandler : IRequestHandler<UploadBannerImageCommand, UploadBannerImageResponse>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;

        public UploadBannerImageHandler(IFileUploadService fileUploadService, IUnitOfWork unitOfWork)
        {
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadBannerImageResponse> Handle(UploadBannerImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ImageFileBytes == null || request.ImageFileBytes.Length == 0 || string.IsNullOrEmpty(request.FileName))
                {
                    return new UploadBannerImageResponse
                    {
                        Success = false,
                        Message = "Lütfen geçerli bir dosya seçin."
                    };
                }

                var imageUrl = await _fileUploadService.UploadImageAsync(request.ImageFileBytes, request.FileName, "banners");

                var banner = await _unitOfWork.Banners.GetByIdAsync(request.BannerId);
                if (banner == null)
                {
                    return new UploadBannerImageResponse
                    {
                        Success = false,
                        Message = "Banner bulunamadı."
                    };
                }

                banner.Update(banner.Title, imageUrl, banner.RedirectUrl);
                _unitOfWork.Banners.Update(banner);
                await _unitOfWork.SaveChangesAsync();

                return new UploadBannerImageResponse
                {
                    Success = true,
                    Message = "Resim başarıyla yüklendi.",
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                return new UploadBannerImageResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
