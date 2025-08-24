using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public RecipeController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: api/recipe
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var recipes = await _unitOfWork.recipes.GetAllAsync();

        var recipeDtos = recipes.Select(r => new RecipeDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Cusine = r.Cusine,
            MealType = r.MealType,
            AstimatedCookingTime = r.AstimatedCokkingTime,
            ImageUrl = r.ImageUrl
        });

        return Ok(recipeDtos);
    }

    // GET: api/recipe/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetails(int id)
    {
        var recipe = await _unitOfWork.recipes.GetByIdWithIncludeAsync(id,
            r => r.Ingredients,
            r => r.Instructions,
            r => r.Ratings);

        if (recipe == null) return NotFound();

        var dto = new RecipeDetailsDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Cusine = recipe.Cusine,
            MealType = recipe.MealType,
            AstimatedCookingTime = recipe.AstimatedCokkingTime,
            ImageUrl = recipe.ImageUrl,
            Ingredients = recipe.Ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Quantity = i.Quantity
            }),
            Instructions = recipe.Instructions.OrderBy(i => i.Order).Select(i => new InstructionDto
            {
                Id = i.Id,
                Order = i.Order,
                StepInstruction = i.StepInstruction
            }),
            Ratings = recipe.Ratings.Select(r => new RatingDto
            {
                Id = r.Id,
                UserId = r.UserId,
                Stars = r.Stars,
                Comment = r.Comment
            })
        };

        return Ok(dto);
    }

    // GET: api/recipe/cuisine/pasta
    [HttpGet("cuisine/{cuisine}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCuisine(string cuisine)
    {
        var recipes = await _unitOfWork.recipes.FindAsync(r => r.Cusine == cuisine);

        var recipeDtos = recipes.Select(r => new RecipeDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Cusine = r.Cusine,
            MealType = r.MealType,
            AstimatedCookingTime = r.AstimatedCokkingTime,
            ImageUrl = r.ImageUrl
        });

        return Ok(recipeDtos);
    }

    // POST, PUT, DELETE remain unchanged
}
