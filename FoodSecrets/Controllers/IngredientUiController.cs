using FoodSecrets.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodSecrets.Controllers
{
    public class IngredientUiController : Controller
    {
        private readonly IIngredientMvc _ingredientService;

        public IngredientUiController(IIngredientMvc ingredientService)
        {
            _ingredientService = ingredientService;
        }

        // List all ingredients
        public async Task<IActionResult> Index()
        {
            var ingredients = await _ingredientService.GetAllAsync();
            return View(ingredients);
        }

        // Show ingredient details
        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _ingredientService.GetByIdAsync(id);
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // Show create form
        public IActionResult Create()
        {
            return View();
        }

        // Handle create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IngredientDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var success = await _ingredientService.CreateAsync(dto);
            if (!success) ModelState.AddModelError("", "Unable to create ingredient.");

            return success ? RedirectToAction(nameof(Index)) : View(dto);
        }

        // Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            var ingredient = await _ingredientService.GetByIdAsync(id);
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // Handle edit POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IngredientDto dto)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid) return View(dto);

            var success = await _ingredientService.UpdateAsync(id, dto);
            if (!success) ModelState.AddModelError("", "Unable to update ingredient.");

            return success ? RedirectToAction(nameof(Index)) : View(dto);
        }

        // Show delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _ingredientService.GetByIdAsync(id);
            if (ingredient == null) return NotFound();
            return View(ingredient);
        }

        // Handle delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _ingredientService.DeleteAsync(id);
            if (!success) return BadRequest("Unable to delete ingredient.");
            return RedirectToAction(nameof(Index));
        }
    }
}
