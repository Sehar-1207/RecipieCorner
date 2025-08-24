using FoodSecrets.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodSecrets.Controllers
{
    public class RecipeUiController : Controller
    {
        private readonly IRecipeMvc _recipeService;

        public RecipeUiController(IRecipeMvc recipeService)
        {
            _recipeService = recipeService;
        }

        // List all recipes
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

        // Show create form
        public IActionResult Create()
        {
            return View();
        }

        // Handle create POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecipeDto dto, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Define the folder path
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "food");

                // Check if folder exists, if not, create it
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Create a unique file name to avoid conflicts
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                // Save the file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // Set the ImageUrl property to be used in the application
                dto.ImageUrl = $"/images/food/{fileName}";
            }

            var success = await _recipeService.CreateAsync(dto);
            if (!success)
                ModelState.AddModelError("", "Unable to create recipe.");

            return success ? RedirectToAction(nameof(Index)) : View(dto);
        }


        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecipeDto dto, IFormFile? ImageFile)
        {
            if (id != dto.Id) return BadRequest();

            if (!ModelState.IsValid) return View(dto);

            // If a new image is uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "food");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(dto.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dto.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                dto.ImageUrl = $"/images/food/{fileName}";
            }

            var success = await _recipeService.UpdateAsync(id, dto);
            if (!success) ModelState.AddModelError("", "Unable to update recipe.");

            return RedirectToAction("Index");
        }

        // Show delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            if (recipe == null) return NotFound();
            return View(recipe);
        }

        // Handle delete POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _recipeService.DeleteAsync(id);
            if (!success) return BadRequest("Unable to delete recipe.");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ByCuisine(string cuisine)
        {
            if (string.IsNullOrEmpty(cuisine))
            {
                return RedirectToAction(nameof(Index));
            }

            var recipes = await _recipeService.GetByCuisineAsync(cuisine);

            return View("Index", recipes); // Reuse the Index view to display filtered recipes
        }

    }
}
