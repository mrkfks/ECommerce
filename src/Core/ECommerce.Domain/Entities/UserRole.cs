namespace ECommerce.Domain.Entities
{
    public class UserRole
    {
        private UserRole() { }

        public int UserId { get; private set; }
        public int RoleId { get; private set; }
        public virtual User? User { get; private set; }
        public virtual Role? Role { get; private set; }
        public string RoleName { get; private set; } = string.Empty;

        public static UserRole Create(int userId, int roleId, string roleName)
        {
            if (userId <= 0)
                throw new ArgumentException("Kullanıcı ID geçersizdir.", nameof(userId));
            
            if (roleId <= 0)
                throw new ArgumentException("Rol ID geçersizdir.", nameof(roleId));
            
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Rol adı boş olamaz.", nameof(roleName));

            return new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                RoleName = roleName
            };
        }
    }
}