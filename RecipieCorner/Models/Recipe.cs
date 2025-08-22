using System.ComponentModel.DataAnnotations;

namespace RecipeCorner.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Cusine { get; set; }

        [Required]
        public string MealType { get; set; }

        [DataType(DataType.Time)]
        public DateTime AstimatedCokkingTime { get; set; }
        
        public string? ImageUrl { get; set; }

        //Relations 
        public ICollection<Instruction> Instructions { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; }
        public ICollection<Rating> Ratings { get; set; }
    }
}
