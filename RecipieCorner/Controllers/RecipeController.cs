using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;

namespace RecipeCorner.Controllers
{
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

            // map entity → dto
            var recipeDtos = recipes.Select(r => new RecipeDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Cusine = r.Cusine,
                MealType = r.MealType,
                AstimatedCokkingTime = r.AstimatedCokkingTime,
                ImageUrl = r.ImageUrl
            });

            return Ok(recipeDtos);
        }

        // GET: api/recipe/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var recipe = await _unitOfWork.recipes.GetByIdAsync(id);
            if (recipe == null)
                return NotFound();

            var dto = new RecipeDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Cusine = recipe.Cusine,
                MealType = recipe.MealType,
                AstimatedCokkingTime = recipe.AstimatedCokkingTime,
                ImageUrl = recipe.ImageUrl
            };

            return Ok(dto);
        }

        // POST: api/recipe
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RecipeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // map dto → entity
            var recipe = new Recipe
            {
                Name = dto.Name,
                Description = dto.Description,
                Cusine = dto.Cusine,
                MealType = dto.MealType,
                AstimatedCokkingTime = dto.AstimatedCokkingTime,
                ImageUrl = dto.ImageUrl
            };

            await _unitOfWork.recipes.AddAsync(recipe);
            await _unitOfWork.SaveAsync();

            // map entity back to dto for response
            dto.Id = recipe.Id;
            return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, dto);
        }

        // PUT: api/recipe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RecipeDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Recipe ID mismatch");

            var existing = await _unitOfWork.recipes.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            // map dto → entity
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Cusine = dto.Cusine;
            existing.MealType = dto.MealType;
            existing.AstimatedCokkingTime = dto.AstimatedCokkingTime;
            existing.ImageUrl = dto.ImageUrl;

            _unitOfWork.recipes.Update(existing);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        // DELETE: api/recipe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _unitOfWork.recipes.GetByIdAsync(id);
            if (recipe == null)
                return NotFound();

            _unitOfWork.recipes.Delete(recipe);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
