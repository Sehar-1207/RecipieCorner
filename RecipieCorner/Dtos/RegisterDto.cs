namespace RecipeCorner.Dtos
{
    public class RegisterDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? SecretKey { get; set; }
        public string? ProfileImage { get; set; }  // uploaded file
    }
}
