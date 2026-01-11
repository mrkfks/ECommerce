namespace ECommerce.Application.DTOs
{
    public class CheckEmailRequest
    {
        public required string Email { get; set; }
    }

    public class CheckUsernameRequest
    {
        public required string Username { get; set; }
    }
}
