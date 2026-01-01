namespace ECommerce.Domain.Entities
{
    public class LoginHistory : BaseEntity
    {
        private LoginHistory() { }

        public int UserId { get; private set; }
        public DateTime LoginTime { get; private set; }
        public string IpAddress { get; private set; } = string.Empty;
        public string? UserAgent { get; private set; }
        public string? Browser { get; private set; }
        public string? OperatingSystem { get; private set; }
        public string? Location { get; private set; }
        public bool IsSuspicious { get; private set; }
        public string? SuspiciousReason { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string? FailureReason { get; private set; }

        public virtual User User { get; private set; } = null!;

        public static LoginHistory Create(
            int userId,
            string ipAddress,
            string? userAgent = null,
            string? browser = null,
            string? operatingSystem = null,
            string? location = null,
            bool isSuccessful = true,
            string? failureReason = null)
        {
            if (userId <= 0)
                throw new ArgumentException("Geçerli bir kullanıcı ID'si girin.", nameof(userId));

            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "Unknown";

            return new LoginHistory
            {
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Browser = browser,
                OperatingSystem = operatingSystem,
                Location = location,
                IsSuspicious = false,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason
            };
        }

        public void MarkAsSuspicious(string reason)
        {
            IsSuspicious = true;
            SuspiciousReason = reason;
            MarkAsModified();
        }

        public void ClearSuspicion()
        {
            IsSuspicious = false;
            SuspiciousReason = null;
            MarkAsModified();
        }

        /// <summary>
        /// Şüpheli giriş tespiti için kurallar:
        /// - Farklı ülkeden giriş
        /// - Çok fazla başarısız giriş denemesi
        /// - Bilinmeyen cihazdan giriş
        /// - VPN/Proxy kullanımı
        /// </summary>
        public void CheckForSuspiciousActivity(
            string? lastKnownIp,
            string? lastKnownLocation,
            int recentFailedAttempts)
        {
            var reasons = new List<string>();

            // Farklı lokasyondan giriş kontrolü
            if (!string.IsNullOrEmpty(lastKnownLocation) &&
                !string.IsNullOrEmpty(Location) &&
                !Location.Equals(lastKnownLocation, StringComparison.OrdinalIgnoreCase))
            {
                reasons.Add($"Farklı lokasyondan giriş: {Location} (Önceki: {lastKnownLocation})");
            }

            // Çok fazla başarısız giriş denemesi
            if (recentFailedAttempts >= 3)
            {
                reasons.Add($"Son 1 saatte {recentFailedAttempts} başarısız giriş denemesi");
            }

            // Çok farklı IP'den giriş
            if (!string.IsNullOrEmpty(lastKnownIp) &&
                !IpAddress.Equals(lastKnownIp, StringComparison.OrdinalIgnoreCase))
            {
                // IP'nin ilk 2 okteti farklıysa şüpheli kabul et
                var lastOctets = lastKnownIp.Split('.');
                var currentOctets = IpAddress.Split('.');

                if (lastOctets.Length >= 2 && currentOctets.Length >= 2)
                {
                    if (lastOctets[0] != currentOctets[0] || lastOctets[1] != currentOctets[1])
                    {
                        reasons.Add($"Farklı ağdan giriş: {IpAddress}");
                    }
                }
            }

            if (reasons.Any())
            {
                MarkAsSuspicious(string.Join("; ", reasons));
            }
        }
    }
}
