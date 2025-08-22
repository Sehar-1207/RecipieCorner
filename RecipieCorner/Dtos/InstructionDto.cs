using System.ComponentModel.DataAnnotations;

public class InstructionDto
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int Order { get; set; }
    [Required]
    public string StepInstruction { get; set; }
    public string? Tip { get; set; }
    public int RecipeId { get; set; }

}