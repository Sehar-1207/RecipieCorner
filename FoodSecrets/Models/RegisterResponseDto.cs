namespace FoodSecrets.Models
{
    public class RegisterResponseDto
    {
        public TokenResponseDto Token { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }

}
