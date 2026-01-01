namespace Dashboard.Web.Services
{
    /// <summary>
    /// Giriş geçmişi API servisi
    /// </summary>
    public class LoginHistoryApiService
    {
        private readonly HttpClient _httpClient;

        public LoginHistoryApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Tüm giriş geçmişini getir
        /// </summary>
        public async Task<List<LoginHistoryVm>> GetAllAsync(int? companyId = null, int? userId = null, int take = 50)
        {
            try
            {
                var url = $"api/LoginHistory?take={take}";
                if (companyId.HasValue) url += $"&companyId={companyId.Value}";
                if (userId.HasValue) url += $"&userId={userId.Value}";

                var result = await _httpClient.GetFromJsonAsync<List<LoginHistoryVm>>(url);
                return result ?? new List<LoginHistoryVm>();
            }
            catch
            {
                return new List<LoginHistoryVm>();
            }
        }

        /// <summary>
        /// Son girişleri getir
        /// </summary>
        public async Task<List<LoginHistoryVm>> GetRecentAsync(int take = 10)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<LoginHistoryVm>>($"api/LoginHistory/recent?take={take}");
                return result ?? new List<LoginHistoryVm>();
            }
            catch
            {
                return new List<LoginHistoryVm>();
            }
        }

        /// <summary>
        /// Şüpheli girişleri getir
        /// </summary>
        public async Task<List<LoginHistoryVm>> GetSuspiciousAsync(int take = 20)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<LoginHistoryVm>>($"api/LoginHistory/suspicious?take={take}");
                return result ?? new List<LoginHistoryVm>();
            }
            catch
            {
                return new List<LoginHistoryVm>();
            }
        }

        /// <summary>
        /// Giriş özeti
        /// </summary>
        public async Task<LoginHistorySummaryVm> GetSummaryAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<LoginHistorySummaryVm>("api/LoginHistory/summary");
                return result ?? new LoginHistorySummaryVm();
            }
            catch
            {
                return new LoginHistorySummaryVm();
            }
        }

        /// <summary>
        /// Şüpheli işareti kaldır
        /// </summary>
        public async Task<bool> ClearSuspicionAsync(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/LoginHistory/{id}/clear-suspicion", null);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    #region ViewModels

    public class LoginHistoryVm
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? UserFullName { get; set; }
        public string? RoleName { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Location { get; set; }
        public bool IsSuspicious { get; set; }
        public string? SuspiciousReason { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; }
        public string TimeAgo { get; set; } = string.Empty;

        /// <summary>
        /// Rol renk sınıfı
        /// </summary>
        public string RoleColorClass => RoleName switch
        {
            "SuperAdmin" => "bg-danger",
            "CompanyAdmin" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };

        /// <summary>
        /// Rol badge rengi
        /// </summary>
        public string RoleBadgeClass => RoleName switch
        {
            "SuperAdmin" => "text-bg-danger",
            "CompanyAdmin" => "text-bg-primary",
            "Customer" => "text-bg-success",
            _ => "text-bg-secondary"
        };
    }

    public class LoginHistorySummaryVm
    {
        public int TotalLogins { get; set; }
        public int SuspiciousLogins { get; set; }
        public int FailedLogins { get; set; }
        public int UniqueUsers { get; set; }
        public List<LoginHistoryVm> RecentLogins { get; set; } = new();
    }

    #endregion
}
