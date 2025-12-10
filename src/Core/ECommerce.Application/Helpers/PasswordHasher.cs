using BCrypt.Net;

namespace ECommerce.Application.Helpers
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12; // Cost parametresi, güvenlik için artırılabilir

        // Şifreyi hashle
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        // Şifreyi doğrula
        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
