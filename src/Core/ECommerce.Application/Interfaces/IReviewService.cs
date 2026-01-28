using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto?> GetByIdAsync(int id);
    Task<IReadOnlyList<ReviewDto>> GetAllAsync();
    Task<IReadOnlyList<ReviewDto>> GetByProductIdAsync(int productId);
    Task<IReadOnlyList<ReviewDto>> GetByCustomerIdAsync(int customerId);
    Task<ReviewDto> AddAsync(ReviewFormDto dto);
    Task UpdateAsync(int id, ReviewFormDto dto);
    Task DeleteAsync(int id);
}
