using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeCorner.Models
{
    public class Instruction
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Order { get; set; }
        [Required]
        public string StepInstruction { get; set; }
        public string? Tip { get; set; }

        [ForeignKey(nameof(Recipe))]
        public int RecipeId { get; set; }
        [ValidateNever]
        public Recipe Recipe { get; set; }
    }
}
