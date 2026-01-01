namespace Dashboard.Web.Services
{
    /// <summary>
    /// Kullanıcı yönetimi API servisi
    /// </summary>
    public class UserManagementApiService
    {
        private readonly HttpClient _httpClient;

        public UserManagementApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Kullanıcı yönetimi özeti
        /// </summary>
        public async Task<UserManagementSummaryVm> GetSummaryAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<UserManagementSummaryVm>("api/UserManagement/summary");
                return result ?? new UserManagementSummaryVm();
            }
            catch
            {
                return new UserManagementSummaryVm();
            }
        }

        /// <summary>
        /// Filtrelenmiş kullanıcı listesi
        /// </summary>
        public async Task<PagedUserListVm> GetAllAsync(UserFilterVm? filter = null)
        {
            try
            {
                filter ??= new UserFilterVm();
                var url = $"api/UserManagement?page={filter.Page}&pageSize={filter.PageSize}";

                if (!string.IsNullOrEmpty(filter.SearchTerm))
                    url += $"&searchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
                if (!string.IsNullOrEmpty(filter.Role))
                    url += $"&role={filter.Role}";
                if (filter.CompanyId.HasValue)
                    url += $"&companyId={filter.CompanyId.Value}";
                if (filter.IsActive.HasValue)
                    url += $"&isActive={filter.IsActive.Value}";

                var result = await _httpClient.GetFromJsonAsync<PagedUserListVm>(url);
                return result ?? new PagedUserListVm();
            }
            catch
            {
                return new PagedUserListVm();
            }
        }

        /// <summary>
        /// Tek kullanıcı detayı
        /// </summary>
        public async Task<UserWithRoleVm?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserWithRoleVm>($"api/UserManagement/{id}");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Kullanıcı rollerini güncelle
        /// </summary>
        public async Task<bool> UpdateRolesAsync(int id, List<string> roles)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/UserManagement/{id}/roles", new { Roles = roles });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kullanıcı aktivasyon durumunu değiştir
        /// </summary>
        public async Task<bool> UpdateActivationAsync(int id, bool isActive)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/UserManagement/{id}/activation", new { UserId = id, IsActive = isActive });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Rol listesi
        /// </summary>
        public async Task<List<RoleVm>> GetRolesAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<List<RoleVm>>("api/UserManagement/roles");
                return result ?? new List<RoleVm>();
            }
            catch
            {
                return new List<RoleVm>();
            }
        }
    }

    #region ViewModels

    public class UserManagementSummaryVm
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int SuperAdminCount { get; set; }
        public int CompanyAdminCount { get; set; }
        public int CustomerCount { get; set; }
        public int TodayLogins { get; set; }
        public int SuspiciousLogins { get; set; }
        public List<UserWithRoleVm> RecentUsers { get; set; } = new();
    }

    public class UserWithRoleVm
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole => Roles.FirstOrDefault() ?? "Customer";
        public DateTime? LastLoginTime { get; set; }
        public string? LastLoginIp { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Rol renk sınıfı
        /// </summary>
        public string RoleColorClass => PrimaryRole switch
        {
            "SuperAdmin" => "bg-danger",
            "CompanyAdmin" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };

        /// <summary>
        /// Rol badge rengi
        /// </summary>
        public string RoleBadgeClass => PrimaryRole switch
        {
            "SuperAdmin" => "text-bg-danger",
            "CompanyAdmin" => "text-bg-primary",
            "Customer" => "text-bg-success",
            _ => "text-bg-secondary"
        };

        /// <summary>
        /// Aktivasyon renk sınıfı
        /// </summary>
        public string StatusBadgeClass => IsActive ? "text-bg-success" : "text-bg-secondary";
        public string StatusText => IsActive ? "Aktif" : "Pasif";
    }

    public class PagedUserListVm
    {
        public List<UserWithRoleVm> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    public class UserFilterVm
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public int? CompanyId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class RoleVm
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>
        /// Rol renk sınıfı
        /// </summary>
        public string RoleColorClass => Name switch
        {
            "SuperAdmin" => "bg-danger",
            "CompanyAdmin" => "bg-primary",
            "Customer" => "bg-success",
            _ => "bg-secondary"
        };
    }

    #endregion
}
