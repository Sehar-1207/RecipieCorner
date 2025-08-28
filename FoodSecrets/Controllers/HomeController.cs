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

        public async Task<IActionResult> Index()
        {
            var recipes = await _recipeService.GetAllAsync();
            return View(recipes);
        }

        // Recipe details
        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _recipeService.GetDetailsAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
