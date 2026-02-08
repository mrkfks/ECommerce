namespace ECommerce.Domain.Enums
{
    public enum ReturnRequestStatus
    {
        Pending = 0,         // Beklemede
        Approved = 1,        // Onaylandı
        Rejected = 2,        // Reddedildi
        Processing = 3,      // İşlemde
        Completed = 4        // Tamamlandı
    }
}
