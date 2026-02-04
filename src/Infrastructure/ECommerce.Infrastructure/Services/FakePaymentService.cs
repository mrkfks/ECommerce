using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Services
{
    public class FakePaymentService : IPaymentService
    {
        public bool ValidatePayment(string cardNumber, string expiry, string cvv)
        {
            // Basit Simülasyon: Test kart numaraları kabul ediliyor
            // Geçerli test kartları: 4444, 5555, 3782, 6011 (standart test kartlar)
            // Boş veya null kart da kabul edilir (simülasyon amaçlı)
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                // Boş kart bilgisi - development için başarılı say
                return true;
            }

            var cleanCardNumber = cardNumber.Trim().Replace(" ", "").Replace("-", "");

            // Test kart numaraları
            var testCardPrefixes = new[] { "4444", "5555", "3782", "6011", "3714", "36" };

            foreach (var prefix in testCardPrefixes)
            {
                if (cleanCardNumber.StartsWith(prefix))
                {
                    return true;
                }
            }

            // Geliştirme ortamında diğer kartları da kabul et (üretimde değiştir)
            if (cleanCardNumber.Length >= 13 && cleanCardNumber.All(char.IsDigit))
            {
                return true;  // Geçerli uzunlukta herhangi bir sayı dizisi kabul edilir
            }

            return false;
        }
    }
}
