using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SameCompanyOrSuperAdmin")]
public class CampaignController : ControllerBase
{
    private readonly AppDbContext _context;

    public CampaignController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var campaigns = await _context.Campaigns
            .Include(c => c.Company)
            .AsNoTracking()
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DiscountPercent = c.DiscountPercent,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                IsCurrentlyActive = c.IsActive && DateTime.UtcNow >= c.StartDate && DateTime.UtcNow <= c.EndDate,
                RemainingDays = c.IsActive && DateTime.UtcNow >= c.StartDate && DateTime.UtcNow <= c.EndDate
                    ? (int)(c.EndDate - DateTime.UtcNow).TotalDays
                    : 0,
                CompanyId = c.CompanyId,
                CompanyName = c.Company != null ? c.Company.Name : "",
                CreatedAt = c.CreatedAt
            })
            .OrderByDescending(c => c.IsCurrentlyActive)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(campaigns);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var now = DateTime.UtcNow;
        var campaigns = await _context.Campaigns
            .Include(c => c.Company)
            .Where(c => c.IsActive && now >= c.StartDate && now <= c.EndDate)
            .AsNoTracking()
            .Select(c => new CampaignDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DiscountPercent = c.DiscountPercent,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                IsCurrentlyActive = true,
                RemainingDays = (int)(c.EndDate - now).TotalDays,
                CompanyId = c.CompanyId,
                CompanyName = c.Company != null ? c.Company.Name : "",
                CreatedAt = c.CreatedAt
            })
            .OrderBy(c => c.EndDate)
            .ToListAsync();

        return Ok(campaigns);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var now = DateTime.UtcNow;
        var campaigns = await _context.Campaigns
            .Include(c => c.Company)
            .AsNoTracking()
            .ToListAsync();

        var summary = new CampaignSummaryDto
        {
            TotalCampaigns = campaigns.Count,
            ActiveCampaigns = campaigns.Count(c => c.IsActive && now >= c.StartDate && now <= c.EndDate),
            UpcomingCampaigns = campaigns.Count(c => c.IsActive && c.StartDate > now),
            ExpiredCampaigns = campaigns.Count(c => c.EndDate < now),
            CurrentCampaigns = campaigns
                .Where(c => c.IsActive && now >= c.StartDate && now <= c.EndDate)
                .Select(c => new CampaignDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    DiscountPercent = c.DiscountPercent,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    IsCurrentlyActive = true,
                    RemainingDays = (int)(c.EndDate - now).TotalDays,
                    CompanyId = c.CompanyId,
                    CompanyName = c.Company?.Name ?? "",
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.EndDate)
                .Take(5)
                .ToList()
        };

        return Ok(summary);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var campaign = await _context.Campaigns
            .Include(c => c.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        var now = DateTime.UtcNow;
        return Ok(new CampaignDto
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            DiscountPercent = campaign.DiscountPercent,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            IsActive = campaign.IsActive,
            IsCurrentlyActive = campaign.IsActive && now >= campaign.StartDate && now <= campaign.EndDate,
            RemainingDays = campaign.IsActive && now >= campaign.StartDate && now <= campaign.EndDate
                ? (int)(campaign.EndDate - now).TotalDays
                : 0,
            CompanyId = campaign.CompanyId,
            CompanyName = campaign.Company?.Name ?? "",
            CreatedAt = campaign.CreatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CampaignCreateDto dto)
    {
        try
        {
            var campaign = Campaign.Create(
                dto.Name,
                dto.DiscountPercent,
                dto.StartDate,
                dto.EndDate,
                dto.CompanyId,
                dto.Description);

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

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
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        try
        {
            campaign.Update(dto.Name, dto.DiscountPercent, dto.StartDate, dto.EndDate, dto.Description);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kampanya başarıyla güncellendi." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        campaign.Activate();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kampanya aktifleştirildi." });
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        campaign.Deactivate();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kampanya pasifleştirildi." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var campaign = await _context.Campaigns.FindAsync(id);
        if (campaign == null)
            return NotFound(new { message = "Kampanya bulunamadı." });

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kampanya silindi." });
    }
}
