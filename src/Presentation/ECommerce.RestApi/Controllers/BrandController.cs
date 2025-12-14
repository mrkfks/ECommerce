using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>
    /// Get all brands
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _brandService.GetAllAsync();
        return Ok(brands);
    }

    /// <summary>
    /// Get brand by id
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var brand = await _brandService.GetByIdAsync(id);
        if (brand == null)
            return NotFound(new { message = "Marka bulunamadı" });
        return Ok(brand);
    }

    /// <summary>
    /// Create a new brand
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] BrandCreateDto dto)
    {
        var brand = await _brandService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = brand.Id }, brand);
    }

    /// <summary>
    /// Update a brand
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] BrandUpdateDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        await _brandService.UpdateAsync(dto);
        return Ok(new { message = "Marka güncellendi" });
    }

    /// <summary>
    /// Delete a brand
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _brandService.DeleteAsync(id);
        return Ok(new { message = "Marka silindi" });
    }
}
