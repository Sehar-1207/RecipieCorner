namespace FoodSecrets.Models
{
    public class AuthResponseDto
    {
        public TokenResponseDto Token { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }  // nullable so it works in both cases
        public List<string> Roles { get; set; } = new List<string>();
    }
}
