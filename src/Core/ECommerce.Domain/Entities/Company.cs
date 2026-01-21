namespace ECommerce.Domain.Entities
{
    public class Company : BaseEntity
    {
        private Company() { }

        public string Name { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string TaxNumber { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public bool IsApproved { get; private set; } = false;
        
        // Sorumlu Kişi Bilgileri
        public string? ResponsiblePersonName { get; private set; }
        public string? ResponsiblePersonPhone { get; private set; }
        public string? ResponsiblePersonEmail { get; private set; }
        
        // Branding & Configuration
        public string? Domain { get; private set; } // e.g. "tenant1.myshop.com"
        public string? LogoUrl { get; private set; }
        public string PrimaryColor { get; private set; } = "#3b82f6"; // Default Blue
        public string SecondaryColor { get; private set; } = "#1e40af"; // Default Dark Blue

        public virtual ICollection<User> Users { get; private set; } = new List<User>();
        public virtual ICollection<Customer> Customers { get; private set; } = new List<Customer>();
        public virtual ICollection<Product> Products { get; private set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
        public virtual ICollection<Request> Requests { get; private set; } = new List<Request>();

        public static Company Create(string name, string address, string phoneNumber, string email, string taxNumber, 
            string? responsiblePersonName = null, string? responsiblePersonPhone = null, string? responsiblePersonEmail = null)
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
                ResponsiblePersonName = responsiblePersonName,
                ResponsiblePersonPhone = responsiblePersonPhone,
                ResponsiblePersonEmail = responsiblePersonEmail,
                IsActive = true,
                IsApproved = false
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
            MarkAsModified();
        }

        public void Approve()
        {
            IsApproved = true;
            MarkAsModified();
        }

        public void Reject()
        {
            IsApproved = false;
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
        public void UpdateBranding(string? domain, string? logoUrl, string? primaryColor, string? secondaryColor)
        {
            Domain = domain?.Trim().ToLower();
            LogoUrl = logoUrl;
            if (!string.IsNullOrWhiteSpace(primaryColor)) PrimaryColor = primaryColor;
            if (!string.IsNullOrWhiteSpace(secondaryColor)) SecondaryColor = secondaryColor;
            MarkAsModified();
        }
    }
}