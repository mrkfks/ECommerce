using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities
{
    public class Order : BaseEntity, ITenantEntity
    {
        private Order() { }

        public int CustomerId { get; private set; }
        public int AddressId { get; private set; }
        public int CompanyId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal TotalAmount { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        
        public virtual Customer? Customer { get; private set; }
        public virtual Address? Address { get; private set; }
        public virtual Company? Company { get; private set; }
        public virtual ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

        public static Order Create(int customerId, int addressId, int companyId)
        {
            if (customerId <= 0)
                throw new ArgumentException("Müşteri ID geçersizdir.", nameof(customerId));
            
            if (addressId <= 0)
                throw new ArgumentException("Adres ID geçersizdir.", nameof(addressId));
            
            if (companyId <= 0)
                throw new ArgumentException("Şirket ID geçersizdir.", nameof(companyId));

            return new Order
            {
                CustomerId = customerId,
                AddressId = addressId,
                CompanyId = companyId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0,
                Status = OrderStatus.Pending
            };
        }

        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Yalnızca Bekleme durumundaki siparişlere ürün eklenebilir.");
            
            Items.Add(item);
            RecalculateTotal();
        }

        public void RemoveItem(int orderItemId)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Yalnızca Bekleme durumundaki siparişlerden ürün kaldırılabilir.");
            
            var item = Items.FirstOrDefault(x => x.Id == orderItemId);
            if (item != null)
            {
                Items.Remove(item);
                RecalculateTotal();
            }
        }

        public void Confirm()
        {
            if (Items.Count == 0)
                throw new InvalidOperationException("En az bir ürün içermesi gereken sipariş onaylanamaz.");
            
            Status = OrderStatus.Processing;
            MarkAsModified();
        }

        public void Ship()
        {
            if (Status != OrderStatus.Processing)
                throw new InvalidOperationException("Yalnızca İşlemde olan siparişler gönderilebilir.");
            
            Status = OrderStatus.Shipped;
            MarkAsModified();
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Yalnızca Gönderilen siparişler teslim edilebilir.");
            
            Status = OrderStatus.Delivered;
            MarkAsModified();
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Delivered || Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Teslim edilen veya iptal edilmiş siparişler iptal edilemez.");
            
            Status = OrderStatus.Cancelled;
            MarkAsModified();
        }

        public void MarkAsPaid()
        {
            Status = OrderStatus.Completed;
            MarkAsModified();
        }

        private void RecalculateTotal()
        {
            TotalAmount = Items.Sum(x => x.UnitPrice * x.Quantity);
        }
    }
}