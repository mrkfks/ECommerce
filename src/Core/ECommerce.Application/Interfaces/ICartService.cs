using System.Threading.Tasks;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.Cart;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string? sessionId = null);
    Task AddToCartAsync(AddToCartDto dto, string? sessionId = null);
    Task RemoveFromCartAsync(int itemId);
    Task UpdateQuantityAsync(int itemId, int quantity);
    Task ClearCartAsync();
    Task MergeCartAsync(string sessionId); // Login olunca guest sepetini birleştir
}
