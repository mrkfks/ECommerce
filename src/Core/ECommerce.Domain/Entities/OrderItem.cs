namespace ECommerce.Domain.Entities
{
    public class OrderItem
    {
        private OrderItem() { }

        public int Id { get; private set; }
        public int ProductId { get; private set; }
        public virtual Product? Product { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public int OrderId { get; private set; }
        public virtual Order? Order { get; private set; }

        public static OrderItem Create(int productId, int quantity, decimal unitPrice)
        {
            if (quantity <= 0)
                throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
            
            if (unitPrice <= 0)
                throw new ArgumentException("Birim fiyat sıfırdan büyük olmalıdır.", nameof(unitPrice));

            return new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(newQuantity));

            Quantity = newQuantity;
        }
    }
}