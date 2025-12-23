namespace ECommerce.Domain.Entities
{
    public class Company : IAuditable
    {
        private Company() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
        public string TaxNumber { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public bool IsApproved { get; private set; } = false;

        public virtual ICollection<User> Users { get; private set; } = new List<User>();
        public virtual ICollection<Customer> Customers { get; private set; } = new List<Customer>();
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
        public virtual ICollection<Request> Requests { get; private set; } = new List<Request>();

        public static Company Create(string name, string address, string phoneNumber, string email, string taxNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Şirket adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Adres boş olamaz.", nameof(address));
            
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Telefon numarası boş olamaz.", nameof(phoneNumber));
            
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));
            
            if (string.IsNullOrWhiteSpace(taxNumber))
                throw new ArgumentException("Vergi numarası boş olamaz.", nameof(taxNumber));

            return new Company
            {
                Name = name,
                Address = address,
                PhoneNumber = phoneNumber,
                Email = email,
                TaxNumber = taxNumber,
                IsActive = true,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void Update(string name, string address, string phoneNumber, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Şirket adı boş olamaz.", nameof(name));
            
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Adres boş olamaz.", nameof(address));
            
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Telefon numarası boş olamaz.", nameof(phoneNumber));
            
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Geçerli bir e-posta adresi girin.", nameof(email));

            Name = name;
            Address = address;
            PhoneNumber = phoneNumber;
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Approve()
        {
            IsApproved = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject()
        {
            IsApproved = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}