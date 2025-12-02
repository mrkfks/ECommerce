namespace ECommerce.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }

        public required ICollection<UserRole> UserRoles { get; set; }
        public Customer? CustomerProfile { get; set; }

    }
}