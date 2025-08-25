using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;

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
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RecipeDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var recipe = new Recipe
        {
            Name = dto.Name,
            Description = dto.Description,
            Cusine = dto.Cusine,
            MealType = dto.MealType,
            AstimatedCokkingTime = dto.AstimatedCookingTime,
            ImageUrl = dto.ImageUrl
        };

        await _unitOfWork.recipes.AddAsync(recipe);
        await _unitOfWork.SaveAsync();

        dto.Id = recipe.Id; // return generated Id
        return CreatedAtAction(nameof(GetDetails), new { id = recipe.Id }, dto);
    }

    // PUT: api/recipe/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] RecipeDto dto)
    {
        if (id != dto.Id) return BadRequest("Recipe ID mismatch.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var recipe = await _unitOfWork.recipes.GetByIdAsync(id);
        if (recipe == null) return NotFound("Recipe not found.");

        recipe.Name = dto.Name;
        recipe.Description = dto.Description;
        recipe.Cusine = dto.Cusine;
        recipe.MealType = dto.MealType;
        recipe.AstimatedCokkingTime = dto.AstimatedCookingTime;
        recipe.ImageUrl = dto.ImageUrl;

        _unitOfWork.recipes.Update(recipe);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }

    // DELETE: api/recipe/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await _unitOfWork.recipes.GetByIdAsync(id);
        if (recipe == null) return NotFound("Recipe not found.");

        _unitOfWork.recipes.Delete(recipe);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
}
