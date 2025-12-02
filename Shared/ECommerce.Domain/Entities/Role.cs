namespace ECommerce.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public required ICollection<UserRole> UserRoles { get; set; }
    }
}

