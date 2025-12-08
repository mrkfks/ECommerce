namespace ECommerce.Domain.Entities
{
    public class OrderItem
    {
        public int Id {get; set;}

        public int ProductId {get; set;}
        public required Product Product {get; set;}

        public int Quantity {get; set;}
        public decimal UnitPrice {get; set;}

        public int OrderId {get; set;}
        public virtual required Order Order {get; set;}
    }
}