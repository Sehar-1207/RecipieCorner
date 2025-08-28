using FoodSecrets.Models;
using FoodSecrets.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodSecrets.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecipeMvc _recipeService;

        public HomeController(ILogger<HomeController> logger, IRecipeMvc recipe)
        {
            _logger = logger;
            _recipeService = recipe;
        }

        // Show recipe details page
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _recipeService.GetDetailsAsync(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }

        // Show all recipes (homepage)
        public async Task<IActionResult> Index()
        {
            var recipes = await _recipeService.GetAllAsync();

            // Extract unique cuisines
            var cuisines = recipes
                .Select(r => r.Cusine)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Cuisines = cuisines;
            ViewBag.ActiveCuisine = null; // no active filter on initial load

            return View(recipes);
        }

        // Full page reload filtering (used if JavaScript disabled)
        public async Task<IActionResult> ByCuisine(string cuisine)
        {
            if (string.IsNullOrEmpty(cuisine))
            {
                return RedirectToAction(nameof(Index));
            }

            var recipes = await _recipeService.GetByCuisineAsync(cuisine);

            // Keep full cuisines list (not just filtered)
            var allRecipes = await _recipeService.GetAllAsync();
            var cuisines = allRecipes
                .Select(r => r.Cusine)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Cuisines = cuisines;
            ViewBag.ActiveCuisine = cuisine; // highlight active filter

            return View("Index", recipes);
        }

        // AJAX filter: returns only recipe cards (partial view)
        public async Task<IActionResult> FilterByCuisine(string cuisine)
        {
            var recipes = string.IsNullOrEmpty(cuisine)
                ? await _recipeService.GetAllAsync()
                : await _recipeService.GetByCuisineAsync(cuisine);

            return PartialView("_RecipeCardsPartial", recipes);
        }
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction(nameof(Index));
            }

            var recipes = await _recipeService.SearchAsync(query);

            // If AJAX call → return partial view
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RecipeCardsPartial", recipes);
            }

            // Otherwise reload full page with results
            ViewBag.Cuisines = recipes
                .Select(r => r.Cusine)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.ActiveCuisine = null;
            ViewBag.SearchQuery = query;

            return View("Index", recipes);
        }

        // Error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
