using FoodSecrets.Models;
using FoodSecrets.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodSecrets.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRecipeMvc _recipe;

        public HomeController(ILogger<HomeController> logger, IRecipeMvc recipe)
        {
            _logger = logger;
            _recipe = recipe;
        }

        public async Task<IActionResult> Index()
        {
            var recipes = await _recipe.GetAllAsync();
            return View(recipes);
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
