using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IBannerService
    {
        Task<ApiResponseDto<IEnumerable<BannerDto>>> GetAllAsync();
        Task<ApiResponseDto<BannerDto>> GetByIdAsync(int id);
        Task<ApiResponseDto<BannerDto>> CreateAsync(BannerFormDto dto);
        Task<ApiResponseDto<BannerDto>> UpdateAsync(int id, BannerFormDto dto);
        Task<ApiResponseDto<bool>> UpdateImageAsync(int id, string imageUrl);
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
