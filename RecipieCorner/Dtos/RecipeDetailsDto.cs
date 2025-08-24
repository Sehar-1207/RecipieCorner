public class RecipeDetailsDto : RecipeDto
{
    public IEnumerable<IngredientDto> Ingredients { get; set; } = new List<IngredientDto>();
    public IEnumerable<InstructionDto> Instructions { get; set; } = new List<InstructionDto>();
    public IEnumerable<RatingDto> Ratings { get; set; } = new List<RatingDto>();
}