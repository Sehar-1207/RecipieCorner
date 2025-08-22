using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;

namespace RecipeCorner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IngredientController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ingredients = await _unitOfWork.ingredients.GetAllAsync();
            var dtos = ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Quantity = i.Quantity,
                RecipeId = i.RecipeId
            });

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ingredient = await _unitOfWork.ingredients.GetByIdAsync(id);
            if (ingredient == null) return NotFound();

            var dto = new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Quantity = ingredient.Quantity,
                RecipeId = ingredient.RecipeId
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IngredientDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Quantity = dto.Quantity,
                RecipeId = dto.RecipeId
            };

            await _unitOfWork.ingredients.AddAsync(ingredient);
            await _unitOfWork.SaveAsync();

            dto.Id = ingredient.Id;
            return CreatedAtAction(nameof(GetById), new { id = ingredient.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] IngredientDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var existing = await _unitOfWork.ingredients.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = dto.Name;
            existing.Quantity = dto.Quantity;
            existing.RecipeId = dto.RecipeId;

            _unitOfWork.ingredients.Update(existing);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _unitOfWork.ingredients.GetByIdAsync(id);
            if (ingredient == null) return NotFound();

            _unitOfWork.ingredients.Delete(ingredient);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
