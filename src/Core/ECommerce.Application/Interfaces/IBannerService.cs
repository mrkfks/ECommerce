using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.Banner;
using ECommerce.Application.DTOs.Common;

namespace ECommerce.Application.Interfaces
{
    public interface IBannerService
    {
        Task<ApiResponseDto<IEnumerable<BannerDto>>> GetAllAsync();
        Task<ApiResponseDto<BannerDto>> GetByIdAsync(int id);
        Task<ApiResponseDto<BannerDto>> CreateAsync(CreateBannerDto dto);
        Task<ApiResponseDto<BannerDto>> UpdateAsync(int id, UpdateBannerDto dto);
        Task<ApiResponseDto<bool>> UpdateImageAsync(int id, string imageUrl);
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
