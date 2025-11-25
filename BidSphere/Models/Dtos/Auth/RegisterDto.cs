namespace BidSphere.Models.Dtos.Auth
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; } // Optional: Admin, User, Guest (defaults to User)
    }
}
