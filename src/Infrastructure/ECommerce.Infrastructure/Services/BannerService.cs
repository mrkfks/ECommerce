using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;

using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services
{
    public class BannerService : IBannerService
    {
        private readonly AppDbContext _context;
        private readonly ITenantService _tenantService;
        private readonly ILogger<BannerService> _logger;

        public BannerService(AppDbContext context, ITenantService tenantService, ILogger<BannerService> logger)
        {
            _context = context;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<ApiResponseDto<IEnumerable<BannerDto>>> GetAllAsync()
        {
            try
            {
                var companyId = _tenantService.GetCompanyId();
                var query = _context.Banners.AsNoTracking().Where(b => !b.IsDeleted);

                if (companyId.HasValue)
                {
                    query = query.Where(b => b.CompanyId == companyId.Value);
                }

                var banners = await query.OrderBy(b => b.Order).ToListAsync();
                
                var bannerDtos = banners.Select(MapToDto).ToList();

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
                var banner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
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

        public async Task<ApiResponseDto<BannerDto>> CreateAsync(BannerFormDto dto)
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

                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();

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

        public async Task<ApiResponseDto<BannerDto>> UpdateAsync(int id, BannerFormDto dto)
        {
            try
            {
                var banner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id);
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

                await _context.SaveChangesAsync();

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
        
        public async Task<ApiResponseDto<bool>> UpdateImageAsync(int id, string imageUrl)
        {
             try
            {
                var banner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id);
                if (banner == null || banner.IsDeleted)
                {
                   return new ApiResponseDto<bool> { Success = false, Message = "Banner Not Found" };
                }
                
                var companyId = _tenantService.GetCompanyId();
                if (companyId.HasValue && banner.CompanyId != companyId.Value)
                     return new ApiResponseDto<bool> { Success = false, Message = "Unauthorized" };
                
                banner.Update(banner.Title, imageUrl, banner.Description, banner.Link, banner.Order);
                await _context.SaveChangesAsync();
                
                return new ApiResponseDto<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating banner image");
                return new ApiResponseDto<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var banner = await _context.Banners.FirstOrDefaultAsync(b => b.Id == id);
                if (banner == null || banner.IsDeleted)
                {
                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Banner bulunamadı"
                    };
                }

                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();

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
}
