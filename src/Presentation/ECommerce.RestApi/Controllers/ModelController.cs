using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/models")]
[Authorize]
public class ModelController : ControllerBase
{
    private readonly IModelService _modelService;

    public ModelController(IModelService modelService)
    {
        _modelService = modelService;
    }

    /// <summary>
    /// Get all models
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var models = await _modelService.GetAllAsync();
        return Ok(models);
    }

    /// <summary>
    /// Get models by brand id
    /// </summary>
    [HttpGet("brand/{brandId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByBrandId(int brandId)
    {
        var models = await _modelService.GetByBrandIdAsync(brandId);
        var response = new ECommerce.Application.Responses.ApiResponse<List<ModelDto>>
        {
            Success = true,
            Data = models.ToList(),
            Message = ""
        };
        return Ok(response);
    }

    /// <summary>
    /// Get model by id
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _modelService.GetByIdAsync(id);
        if (model == null)
            return NotFound(new { message = "Model bulunamadı" });
        return Ok(model);
    }

    /// <summary>
    /// Create a new model
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] ModelFormDto dto)
    {
        try
        {
            var model = await _modelService.CreateAsync(dto);
            return Ok(new ECommerce.Application.Responses.ApiResponse<ModelDto>
            {
                Success = true,
                Data = model,
                Message = ""
            });
        }
        catch (Exception ex)
        {
            return Ok(new ECommerce.Application.Responses.ApiResponse<ModelDto>
            {
                Success = false,
                Data = null,
                Message = $"Model eklenirken hata oluştu: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Update a model
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] ModelFormDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Id mismatch");

        await _modelService.UpdateAsync(dto);
        return Ok(new { message = "Model güncellendi" });
    }

    /// <summary>
    /// Delete a model
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _modelService.DeleteAsync(id);
        return Ok(new { message = "Model silindi" });
    }
}
