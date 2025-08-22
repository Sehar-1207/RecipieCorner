using System.ComponentModel.DataAnnotations;

public class IngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Quantity { get; set; }
    public int RecipeId { get; set; }
}