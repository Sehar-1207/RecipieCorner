using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeCorner.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Quantity { get; set; }

        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }

        [ValidateNever]
        public Recipe Recipe { get; set; }
    }
}
