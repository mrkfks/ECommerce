namespace ECommerce.Domain.Entities
{
    public class User : BaseEntity, ITenantEntity
    {
        protected User() { }

        public int CompanyId { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? PhoneNumber { get; private set; }
        public bool IsActive { get; private set; } = true;

        public Company Company { get; private set; } = null!;
        public virtual Customer? CustomerProfile { get; private set; }
        public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        public static User Create(int companyId, string username, string email, string passwordHash, string? firstName = null, string? lastName = null, string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Kullanıcı adı boş olamaz.", nameof(username));

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Şifre boş olamaz.", nameof(passwordHash));

            return new User
            {
                CompanyId = companyId,
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                IsActive = true
            };
        }

        public static User CreateSuperAdmin(string email, string passwordHash, string firstName, string lastName)
        {
            return new User
            {
                Email = email,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateProfile(string? firstName, string? lastName, string email, string? username = null, string? phoneNumber = null)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));

            if (!string.IsNullOrWhiteSpace(username))
            {
                if (username.Length < 3)
                    throw new ArgumentException("Kullanıcı adı en az 3 karakter olmalıdır.", nameof(username));
                Username = username;
            }

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            MarkAsModified();
        }

        public void UpdateCompany(int companyId)
        {
            if (companyId <= 0)
                throw new ArgumentException("Geçerli bir şirket ID'si girin.", nameof(companyId));

            CompanyId = companyId;
            MarkAsModified();
        }

        public void UpdatePhone(string? phoneNumber)
        {
            PhoneNumber = phoneNumber;
            MarkAsModified();
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Şifre boş olamaz.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            MarkAsModified();
        }

        public void Activate()
        {
            IsActive = true;
            MarkAsModified();
        }

        public void Deactivate()
        {
            IsActive = false;
            MarkAsModified();
        }
    }
}