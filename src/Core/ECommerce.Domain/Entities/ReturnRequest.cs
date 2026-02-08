using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities
{
    public class ReturnRequest : BaseEntity, ITenantEntity
    {
        private ReturnRequest() { }

        public int OrderId { get; private set; }
        public int OrderItemId { get; private set; }
        public int ProductId { get; private set; }
        public int CustomerId { get; private set; }
        public int CompanyId { get; private set; }
        public int Quantity { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public string? Comments { get; private set; }
        public ReturnRequestStatus Status { get; private set; } = ReturnRequestStatus.Pending;
        public string? AdminResponse { get; private set; }
        public DateTime RequestDate { get; private set; }
        public DateTime? ResolutionDate { get; private set; }

        // Navigation Properties
        public virtual Order? Order { get; private set; }
        public virtual OrderItem? OrderItem { get; private set; }
        public virtual Product? Product { get; private set; }
        public virtual Customer? Customer { get; private set; }
        public virtual Company? Company { get; private set; }

        public static ReturnRequest Create(
            int orderId,
            int orderItemId,
            int productId,
            int customerId,
            int companyId,
            int quantity,
            string reason,
            string? comments = null)
        {
            if (orderId <= 0)
                throw new ArgumentException("Sipariş ID geçersizdir.", nameof(orderId));

            if (orderItemId <= 0)
                throw new ArgumentException("Sipariş Öğesi ID geçersizdir.", nameof(orderItemId));

            if (productId <= 0)
                throw new ArgumentException("Ürün ID geçersizdir.", nameof(productId));

            if (customerId <= 0)
                throw new ArgumentException("Müşteri ID geçersizdir.", nameof(customerId));

            if (companyId <= 0)
                throw new ArgumentException("Şirket ID geçersizdir.", nameof(companyId));

            if (quantity <= 0)
                throw new ArgumentException("Miktar 0'dan büyük olmalıdır.", nameof(quantity));

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("İade sebebi gereklidir.", nameof(reason));

            return new ReturnRequest
            {
                OrderId = orderId,
                OrderItemId = orderItemId,
                ProductId = productId,
                CustomerId = customerId,
                CompanyId = companyId,
                Quantity = quantity,
                Reason = reason,
                Comments = comments,
                Status = ReturnRequestStatus.Pending,
                RequestDate = DateTime.UtcNow
            };
        }

        public void Approve(string? response = null)
        {
            if (Status != ReturnRequestStatus.Pending)
                throw new InvalidOperationException("Yalnızca Beklemede olan iade talepleri onaylanabilir.");

            Status = ReturnRequestStatus.Approved;
            AdminResponse = response;
            ResolutionDate = DateTime.UtcNow;
            MarkAsModified();
        }

        public void Reject(string? response = null)
        {
            if (Status != ReturnRequestStatus.Pending)
                throw new InvalidOperationException("Yalnızca Beklemede olan iade talepleri reddedilebilir.");

            Status = ReturnRequestStatus.Rejected;
            AdminResponse = response;
            ResolutionDate = DateTime.UtcNow;
            MarkAsModified();
        }

        public void MarkAsProcessing(string? response = null)
        {
            if (Status != ReturnRequestStatus.Approved)
                throw new InvalidOperationException("Yalnızca Onaylanmış iade talepleri işleme alınabilir.");

            Status = ReturnRequestStatus.Processing;
            if (!string.IsNullOrWhiteSpace(response))
                AdminResponse = response;
            MarkAsModified();
        }

        public void Complete(string? response = null)
        {
            if (Status != ReturnRequestStatus.Processing)
                throw new InvalidOperationException("Yalnızca İşlemde olan iade talepleri tamamlanabilir.");

            Status = ReturnRequestStatus.Completed;
            if (!string.IsNullOrWhiteSpace(response))
                AdminResponse = response;
            ResolutionDate = DateTime.UtcNow;
            MarkAsModified();
        }

        public string GetStatusDisplay() => Status switch
        {
            ReturnRequestStatus.Pending => "Beklemede",
            ReturnRequestStatus.Approved => "Onaylandı",
            ReturnRequestStatus.Rejected => "Reddedildi",
            ReturnRequestStatus.Processing => "İşlemde",
            ReturnRequestStatus.Completed => "Tamamlandı",
            _ => "Bilinmeyen"
        };
    }
}
