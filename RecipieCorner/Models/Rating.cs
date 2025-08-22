using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeCorner.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }
        public int Stars {  get; set; }
        public string? Comment { get; set; }
        public DateTime commentAt { get; set; }

        [Required]
        public string UserId { get; set; }  // FK to IdentityUser
        public ApplicationUser User { get; set; }
        [ForeignKey(nameof(Recipe))]
        public int RecipeId { get; set; }
        [ValidateNever]
        public Recipe Recipe {  get; set; }
    }
}
