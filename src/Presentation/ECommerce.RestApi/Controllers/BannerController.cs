using ECommerce.Application.DTOs.Banner;
using ECommerce.Application.DTOs.Common;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers
{
    [ApiController]
    [Route("api/banners")]
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponseDto<IEnumerable<BannerDto>>>> GetAll()
        {
            var result = await _bannerService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponseDto<BannerDto>>> GetById(int id)
        {
            var result = await _bannerService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<BannerDto>>> Create([FromBody] CreateBannerDto dto)
        {
            var result = await _bannerService.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<BannerDto>>> Update(int id, [FromBody] UpdateBannerDto dto)
        {
            var result = await _bannerService.UpdateAsync(id, dto);
            if (!result.Success)
                return BadRequest(result);
            
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<ApiResponseDto<bool>>> Delete(int id)
        {
            var result = await _bannerService.DeleteAsync(id);
            if (!result.Success)
                return NotFound(result);
            
            return Ok(result);
        }
    }
}
