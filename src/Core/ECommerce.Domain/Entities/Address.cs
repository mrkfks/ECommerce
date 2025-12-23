namespace ECommerce.Domain.Entities
{
    public class Address
    {
        private Address() { }

        public int Id { get; private set; }
        public int CustomerId { get; private set; }
        public string Street { get; private set; } = string.Empty;
        public string City { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string ZipCode { get; private set; } = string.Empty;
        public string Country { get; private set; } = string.Empty;
        
        public virtual Customer? Customer { get; private set; }
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

        public static Address Create(int customerId, string street, string city, string state, string zipCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Sokak adı boş olamaz.", nameof(street));
            
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("Şehir boş olamaz.", nameof(city));
            
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("İl boş olamaz.", nameof(state));
            
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Posta kodu boş olamaz.", nameof(zipCode));
            
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Ülke boş olamaz.", nameof(country));

            return new Address
            {
                CustomerId = customerId,
                Street = street,
                City = city,
                State = state,
                ZipCode = zipCode,
                Country = country
            };
        }

        public void Update(string street, string city, string state, string zipCode, string country)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Sokak adı boş olamaz.", nameof(street));
            
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("Şehir boş olamaz.", nameof(city));
            
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("İl boş olamaz.", nameof(state));
            
            if (string.IsNullOrWhiteSpace(zipCode))
                throw new ArgumentException("Posta kodu boş olamaz.", nameof(zipCode));
            
            if (string.IsNullOrWhiteSpace(country))
                throw new ArgumentException("Ülke boş olamaz.", nameof(country));

            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
            Country = country;
        }
    }
}