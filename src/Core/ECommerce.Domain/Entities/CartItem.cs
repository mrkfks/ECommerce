namespace ECommerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public bool IsActive { get; private set; } = true;

    public static CartItem Create(int cartId, int productId, int quantity, decimal unitPrice)
    {
        return new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            IsActive = true
        };
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 1) return;
        Quantity = quantity;
    }
}
