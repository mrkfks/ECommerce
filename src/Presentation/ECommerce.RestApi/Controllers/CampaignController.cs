using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CampaignController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var campaigns = await _campaignService.GetAllAsync();
        return Ok(campaigns);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var campaigns = await _campaignService.GetActiveAsync();
        return Ok(campaigns);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _campaignService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        return Ok(campaign);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CampaignCreateDto dto)
    {
        try
        {
            var campaign = await _campaignService.CreateAsync(dto);
            return Ok(new { id = campaign.Id, message = "Kampanya başarıyla oluşturuldu." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CampaignUpdateDto dto)
    {
        try
        {
            await _campaignService.UpdateAsync(id, dto);
            return Ok(new { message = "Kampanya başarıyla güncellendi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _campaignService.ActivateAsync(id);
            return Ok(new { message = "Kampanya aktifleştirildi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _campaignService.DeactivateAsync(id);
            return Ok(new { message = "Kampanya pasifleştirildi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _campaignService.DeleteAsync(id);
            return Ok(new { message = "Kampanya silindi." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Kampanya bulunamadı." });
        }
    }
}
