using System.ComponentModel.DataAnnotations;

namespace RecipeCorner.Dtos
{
    public class RattingSubmisionDto
    {
        [Required]
        public int RecipeId { get; set; }

        public int? RatingId { get; set; } // Null if creating, has value if updating

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Stars { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }
    }
}
