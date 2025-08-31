using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodSecrets.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class RatingUiController : Controller
    {
        private readonly ApiClientService _apiClient;

        public RatingUiController(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        // ✅ List ratings for a recipe 
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


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _apiClient.GetAsync<RatingDto>($"api/Rating/{id}");
            if (rating == null) return NotFound();

            return View(rating);
        }

        // POST: Delete confirmed 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(RatingDto dto)
        {
            try
            {
                await _apiClient.DeleteAsync($"api/Rating/{dto.Id}");
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    TempData["ErrorMessage"] = "You do not have permission to delete this rating.";
                }
                else if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "The rating you tried to delete was not found.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                }
                // On failure, redirect back to the recipe page, not the delete confirmation.
                return RedirectToAction("Details", "RecipeUi", new { id = dto.RecipeId });
            }

            TempData["SuccessMessage"] = "Rating deleted successfully.";
            return RedirectToAction("Details", "RecipeUi", new { id = dto.RecipeId });
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrUpdate([FromBody] RatingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dto.UserName = User.Identity.Name;
            dto.ProfileImageUrl = ""; // optional
            dto.commentAt = DateTime.UtcNow;

            try
            {
                RatingDto savedRating;
                if (dto.Id > 0)
                {
                    // Update existing review
                    savedRating = await _apiClient.PutAsync<RatingDto>($"api/Rating/{dto.Id}", dto);
                }
                else
                {
                    // Create new review
                    savedRating = await _apiClient.PostAsync<RatingDto>("api/Rating", dto);
                }

                if (savedRating == null)
                    return BadRequest("Failed to save rating.");

                return Json(savedRating); // ✅ Return JSON explicitly
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Conflict("You have already submitted a review for this recipe.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving rating: {ex.Message}");
            }
        }

    }
}
