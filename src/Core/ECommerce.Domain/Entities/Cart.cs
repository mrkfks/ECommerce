using System.Collections.Generic;
using System.Linq;

namespace ECommerce.Domain.Entities;

public class Cart : BaseEntity, ITenantEntity
{
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // For guest users
    public string? SessionId { get; set; }

    public int CompanyId { get; set; }
    public Company? Company { get; set; }

    public ICollection<CartItem> Items { get; private set; } = new List<CartItem>();

    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public bool IsActive { get; private set; } = true;

    public static Cart Create(int companyId, int? customerId = null, string? sessionId = null)
    {
        return new Cart
        {
            CompanyId = companyId,
            CustomerId = customerId,
            SessionId = sessionId,
            IsActive = true
        };
    }

    public void AddItem(int productId, int quantity, decimal unitPrice)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            Items.Add(CartItem.Create(Id, productId, quantity, unitPrice));
        }
    }

    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void ClearItems()
    {
        Items.Clear();
    }
}
