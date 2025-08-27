using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Dtos;
using RecipeCorner.Models;

namespace FoodSecrets.Controllers
{
    [Authorize] // Only logged-in users can access create/update/delete
    public class RatingUiController : Controller
    {
        private readonly ApiClientService _apiClient;

        public RatingUiController(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // ✅ List ratings for a recipe (anyone can view)
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(int recipeId)
        {
            var ratings = await _apiClient.GetAsync<List<RatingDto>>($"api/Rating?recipeId={recipeId}");
            ViewBag.RecipeId = recipeId;
            return View(ratings ?? new List<RatingDto>());
        }

        // ✅ GET: Create rating page
        [HttpGet]
        public async Task<IActionResult> Create(int recipeId)
        {
            // --- ADD THIS LOGIC ---
            // Fetch the recipe from your API to get its name
            var recipe = await _apiClient.GetAsync<RecipeDto>($"api/Recipe/{recipeId}");
            if (recipe == null)
            {
                return NotFound(); // Or handle the error appropriately
            }

            ViewBag.RecipeName = recipe.Name;

            return View(new RatingDto { RecipeId = recipeId });
        }

        // ✅ POST: Create rating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RatingDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var payload = new { dto.RecipeId, dto.Stars, dto.Comment , dto.commentAt};
                var result = await _apiClient.PostAsync<RatingDto>("api/Rating", payload);

                if (result == null)
                {
                    ModelState.AddModelError("", "Failed to submit rating. Make sure you are logged in.");
                    return View(dto);
                }

                return RedirectToAction("Index", new { recipeId = dto.RecipeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error submitting rating: {ex.Message}");
                return View(dto);
            }
        }

        // ✅ GET: Edit rating page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Fetch the rating by ID
                var rating = await _apiClient.GetAsync<RatingDto>($"api/Rating/{id}");

                if (rating == null)
                    return NotFound(); // Rating not found

                return View(rating);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(); // Gracefully handle 404 from API
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fetching rating: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: Edit rating
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RatingDto dto)
        {
            if (id != dto.Id)
                return BadRequest();

            var payload = new { dto.Id, dto.RecipeId, dto.Stars, dto.Comment };

            try
            {
                var result = await _apiClient.PutAsync<RatingDto>($"api/Rating/{id}", payload);

                if (result == null)
                {
                    ModelState.AddModelError("", "Failed to update rating. Make sure the rating exists.");
                    return View(dto);
                }

                return RedirectToAction("Index", new { recipeId = dto.RecipeId });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(); // Rating was deleted or not found
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating rating: {ex.Message}");
                return View(dto);
            }
        }


        // ✅ GET: Delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _apiClient.GetAsync<RatingDto>($"api/Rating/{id}");
            if (rating == null) return NotFound();

            return View(rating);
        }

        // ✅ POST: Delete confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int recipeId)
        {
            try
            {
                var success = await _apiClient.DeleteAsync($"api/Rating/{id}");
                if (!success)
                {
                    ModelState.AddModelError("", "Failed to delete rating");
                    return RedirectToAction("Delete", new { id });
                }
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return RedirectToAction("Delete", new { id });
            }

            return RedirectToAction("Index", new { recipeId });
        }

    }
}
