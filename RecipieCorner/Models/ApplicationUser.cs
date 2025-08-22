using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RecipeCorner.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string FullName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public string? ProfileImageUrl { get; set; }   // user profile picture

        // navigation
        [ValidateNever]
        public ICollection<Rating> Ratings { get; set; }
    }
}
