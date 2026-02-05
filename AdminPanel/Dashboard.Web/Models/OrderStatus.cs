namespace Dashboard.Web.Models
{
    public enum OrderStatus
    {
        Pending,      // Beklemede - Sipariş alındı, onay bekleniyor
        Processing,   // İşleniyor - Onaylandı, hazırlanıyor
        Shipped,      // Kargoda - Kargoya verildi
        Delivered,    // Teslim Edildi - Müşteriye ulaştı
        Cancelled,    // İptal Edildi
        Received,     // Teslim Alındı - Müşteri onayladı
        Completed     // Tamamlandı - Süreç bitti
    }
}
