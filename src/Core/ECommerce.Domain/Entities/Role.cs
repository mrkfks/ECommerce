namespace ECommerce.Domain.Entities
{
    public class Role
    {
        private Role() { }

        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();

        public static Role Create(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Rol adı boş olamaz.", nameof(name));

            return new Role
            {
                Name = name,
                Description = description ?? string.Empty
            };
        }

        public void Update(string name, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Rol adı boş olamaz.", nameof(name));

            Name = name;
            Description = description ?? string.Empty;
        }
    }
}

