using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] string? sessionId)
    {
        var cart = await _cartService.GetCartAsync(sessionId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto, [FromQuery] string? sessionId)
    {
        await _cartService.AddToCartAsync(dto, sessionId);
        return Ok(new { message = "Item added to cart" });
    }

    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateItem(int itemId, [FromBody] UpdateCartItemDto dto)
    {
        await _cartService.UpdateQuantityAsync(itemId, dto.Quantity);
        return Ok(new { message = "Item quantity updated" });
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        await _cartService.RemoveFromCartAsync(itemId);
        return Ok(new { message = "Item removed from cart" });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync();
        return Ok(new { message = "Cart cleared" });
    }

    [HttpPost("merge")]
    public async Task<IActionResult> MergeCart([FromBody] MergeCartDto dto)
    {
        await _cartService.MergeCartAsync(dto.SessionId);
        return Ok(new { message = "Carts merged" });
    }
}

public class MergeCartDto
{
    public string SessionId { get; set; } = string.Empty;
}
