using ECommerce.Application.DTOs.Banner;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services
{
    public class BannerService : IBannerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantService _tenantService;
        private readonly ILogger<BannerService> _logger;

        public BannerService(IUnitOfWork unitOfWork, ITenantService tenantService, ILogger<BannerService> logger)
        {
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<ApiResponseDto<IEnumerable<BannerDto>>> GetAllAsync()
        {
            try
            {
                var companyId = _tenantService.GetCompanyId();
                var banners = await _unitOfWork.Banners.GetAllAsync();

                var filteredBanners = companyId.HasValue
                    ? banners.Where(b => !b.IsDeleted && b.CompanyId == companyId.Value)
                    : banners.Where(b => !b.IsDeleted);

                var bannerDtos = filteredBanners
                    .OrderBy(b => b.Order)
                    .Select(MapToDto)
                    .ToList();

                return new ApiResponseDto<IEnumerable<BannerDto>>
                {
                    Success = true,
                    Data = bannerDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banners");
                return new ApiResponseDto<IEnumerable<BannerDto>>
                {
                    Success = false,
                    Message = "Banner'lar alınırken hata oluştu"
                };
            }
        }

        public async Task<ApiResponseDto<BannerDto>> GetByIdAsync(int id)
        {
            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null || banner.IsDeleted)
                {
                    return new ApiResponseDto<BannerDto>
                    {
                        Success = false,
                        Message = "Banner bulunamadı"
                    };
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && banner.CompanyId != companyId.Value)
                {
                    return new ApiResponseDto<BannerDto>
                    {
                        Success = false,
                        Message = "Bu banner'a erişim yetkiniz yok"
                    };
                }

                return new ApiResponseDto<BannerDto>
                {
                    Success = true,
                    Data = MapToDto(banner)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting banner {Id}", id);
                return new ApiResponseDto<BannerDto>
                {
                    Success = false,
                    Message = "Banner alınırken hata oluştu"
                };
            }
        }

        public async Task<ApiResponseDto<BannerDto>> CreateAsync(CreateBannerDto dto)
        {
            try
            {
                var companyId = _tenantService.GetCurrentCompanyId();
                
                var banner = Banner.Create(
                    dto.Title,
                    dto.ImageUrl,
                    companyId,
                    dto.Description,
                    dto.Link,
                    dto.Order
                );

                await _unitOfWork.Banners.AddAsync(banner);
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponseDto<BannerDto>
                {
                    Success = true,
                    Data = MapToDto(banner),
                    Message = "Banner başarıyla oluşturuldu"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating banner");
                return new ApiResponseDto<BannerDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponseDto<BannerDto>> UpdateAsync(int id, UpdateBannerDto dto)
        {
            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null || banner.IsDeleted)
                {
                    return new ApiResponseDto<BannerDto>
                    {
                        Success = false,
                        Message = "Banner bulunamadı"
                    };
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && banner.CompanyId != companyId.Value)
                {
                    return new ApiResponseDto<BannerDto>
                    {
                        Success = false,
                        Message = "Bu banner'ı düzenleme yetkiniz yok"
                    };
                }

                banner.Update(dto.Title, dto.ImageUrl, dto.Description, dto.Link, dto.Order);
                
                if (dto.IsActive)
                    banner.Activate();
                else
                    banner.Deactivate();

                await _unitOfWork.SaveChangesAsync();

                return new ApiResponseDto<BannerDto>
                {
                    Success = true,
                    Data = MapToDto(banner),
                    Message = "Banner başarıyla güncellendi"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner {Id}", id);
                return new ApiResponseDto<BannerDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var banner = await _unitOfWork.Banners.GetByIdAsync(id);
                if (banner == null || banner.IsDeleted)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Banner bulunamadı"
                    };
                }

                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && banner.CompanyId != companyId.Value)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Bu banner'ı silme yetkiniz yok"
                    };
                }

                banner.Delete();
                await _unitOfWork.SaveChangesAsync();

                return new ApiResponseDto<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Banner başarıyla silindi"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting banner {Id}", id);
                return new ApiResponseDto<bool>
                {
                    Success = false,
                    Message = "Banner silinirken hata oluştu"
                };
            }
        }

        private static BannerDto MapToDto(Banner banner)
        {
            return new BannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                ImageUrl = banner.ImageUrl,
                Link = banner.Link,
                Order = banner.Order,
                IsActive = banner.IsActive,
                CompanyId = banner.CompanyId,
                CreatedAt = banner.CreatedAt
            };
        }
    }

    public interface IBannerService
    {
        Task<ApiResponseDto<IEnumerable<BannerDto>>> GetAllAsync();
        Task<ApiResponseDto<BannerDto>> GetByIdAsync(int id);
        Task<ApiResponseDto<BannerDto>> CreateAsync(CreateBannerDto dto);
        Task<ApiResponseDto<BannerDto>> UpdateAsync(int id, UpdateBannerDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
