namespace ECommerce.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        public Company Company { get; set; } = null!;
        public virtual Customer? CustomerProfile { get; set; }
        public virtual required ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}