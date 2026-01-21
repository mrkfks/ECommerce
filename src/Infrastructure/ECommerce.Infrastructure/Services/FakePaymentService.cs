using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Services
{
    public class FakePaymentService : IPaymentService
    {
        public bool ValidatePayment(string cardNumber, string expiry, string cvv)
        {
            // Basit Simülasyon: Kart numarası "4444" ile başlıyorsa başarılı
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Trim().StartsWith("4444"))
            {
                return true;
            }

            return false;
        }
    }
}
