using System.ComponentModel.DataAnnotations;

public class RecipeDto
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
}
