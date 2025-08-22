namespace FoodSecrets.Models
{
    public class LoginResponseDto
    {
        public TokenResponseDto Token { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; }
        public string? ProfileImageUrl { get; set; }
    }

}
