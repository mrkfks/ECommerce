namespace ECommerce.Domain.Entities
{
    public class Customer : BaseEntity, ITenantEntity
    {
        private Customer() { }

        public int CompanyId { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public int? UserId { get; private set; }
        public string PhoneNumber { get; private set; } = string.Empty;
        public DateTime DateOfBirth { get; private set; }

        public virtual User? User { get; private set; }
        public virtual Company? Company { get; private set; }
        public virtual ICollection<Address>? Addresses { get; private set; } = new List<Address>();
        public virtual ICollection<Review>? Reviews { get; private set; } = new List<Review>();
        public virtual ICollection<Order>? Orders { get; private set; } = new List<Order>();

        public static Customer Create(int companyId, string firstName, string lastName, string email, string phoneNumber, DateTime dateOfBirth, int? userId = null)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Ad boş olamaz.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Soyadı boş olamaz.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Telefon numarası boş olamaz.", nameof(phoneNumber));

            return new Customer
            {
                CompanyId = companyId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber,
                DateOfBirth = dateOfBirth,
                UserId = userId
            };
        }

        public void Update(string firstName, string lastName, string email, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Ad boş olamaz.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Soyadı boş olamaz.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Telefon numarası boş olamaz.", nameof(phoneNumber));

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void LinkUser(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Kullanıcı ID geçersizdir.", nameof(userId));

            UserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}