using System.ComponentModel.DataAnnotations;

public class UpdateProfile
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; } // existing image path
    public IFormFile? ProfileImage { get; set; }
    public string Email { get; set; } = string.Empty;  // <-- add this
}
