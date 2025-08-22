using FoodSecrets.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodSecrets.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IRecipe _recipeService;

        public RecipeController(IRecipe recipeService)
        {
            _recipeService = recipeService;
        }

        public async Task<IActionResult> Index()
        {
            var recipes = await _recipeService.GetAllAsync();
            return View(recipes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }
    }
}
