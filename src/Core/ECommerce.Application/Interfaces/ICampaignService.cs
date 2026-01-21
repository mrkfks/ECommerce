using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface ICampaignService
{
    Task<IReadOnlyList<CampaignDto>> GetAllAsync();
    Task<IReadOnlyList<CampaignDto>> GetActiveAsync();
    Task<CampaignSummaryDto> GetSummaryAsync();
    Task<CampaignDto?> GetByIdAsync(int id);
    Task<CampaignDto> CreateAsync(CampaignCreateDto dto);
    Task UpdateAsync(int id, CampaignUpdateDto dto);
    Task ActivateAsync(int id);
    Task DeactivateAsync(int id);
    Task DeleteAsync(int id);
}
