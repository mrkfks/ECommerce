using ECommerce.Application.Features.Category.Commands;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Features.Category.Handlers
{
    public class UploadCategoryImageHandler : IRequestHandler<UploadCategoryImageCommand, UploadCategoryImageResponse>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;

        public UploadCategoryImageHandler(IFileUploadService fileUploadService, IUnitOfWork unitOfWork)
        {
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
        }

        public async Task<UploadCategoryImageResponse> Handle(UploadCategoryImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ImageFileBytes == null || request.ImageFileBytes.Length == 0 || string.IsNullOrEmpty(request.FileName))
                {
                    return new UploadCategoryImageResponse
                    {
                        Success = false,
                        Message = "Lütfen geçerli bir dosya seçin."
                    };
                }

                var imageUrl = await _fileUploadService.UploadImageAsync(request.ImageFileBytes, request.FileName, "categories");

                var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
                if (category == null)
                {
                    return new UploadCategoryImageResponse
                    {
                        Success = false,
                        Message = "Kategori bulunamadı."
                    };
                }

                category.Update(category.Name, category.Description, imageUrl);
                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                return new UploadCategoryImageResponse
                {
                    Success = true,
                    Message = "Resim başarıyla yüklendi.",
                    ImageUrl = imageUrl
                };
            }
            catch (Exception ex)
            {
                return new UploadCategoryImageResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
