using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;
using System.Security.Claims;

namespace RecipeCorner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        private static RatingDto ToDto(Rating r) => new RatingDto
        {
            Id = r.Id,
            Stars = r.Stars,
            Comment = r.Comment,
            RecipeId = r.RecipeId,
            UserId = r.UserId,
            UserName = r.User?.UserName,
            ProfileImageUrl = r.User?.ProfileImageUrl,
            commentAt = r.commentAt,
            RecipeName = r.Recipe?.Name // <-- Map the recipe name here
        };

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var rating = await _unitOfWork.rating.GetByIdWithIncludeAsync(id,
      r => r.User,   // Include the User
      r => r.Recipe  // Also include the Recipe
  );
            if (rating == null) return NotFound();
            return Ok(ToDto(rating));
        }

        // --- UPDATE THE GetAll METHOD ---
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            // Modify the query to also include the Recipe entity
            var ratings = await _unitOfWork.rating.GetAllWithIncludeAsync(
                r => r.User,   // Include the User
                r => r.Recipe  // Include the Recipe
            );

            return Ok(ratings.Select(ToDto));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] RatingDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var existing = await _unitOfWork.rating.GetByIdWithIncludeAsync(id, r => r.User);
            if (existing == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existing.UserId != userId) return Forbid();

            existing.Stars = dto.Stars;
            existing.Comment = dto.Comment;
            existing.commentAt = DateTime.UtcNow;

            _unitOfWork.rating.Update(existing);
            await _unitOfWork.SaveAsync();

            return Ok(ToDto(existing));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Find the rating in the database.
            var rating = await _unitOfWork.rating.GetByIdWithIncludeAsync(id, r => r.User);
            if (rating == null)
            {
                return NotFound();
            }

            // 2. Get the current user's ID and check if they are an Admin.
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isCurrentUserAdmin = User.IsInRole("Admin");

            // 3. ✅ THE FIX IS HERE: The Security Check
            // Block the action ONLY IF the user is NOT the owner AND is NOT an Admin.
            if (rating.UserId != currentUserId && !isCurrentUserAdmin)
            {
                // This returns a 403 Forbidden error if a regular user tries to
                // delete a rating that does not belong to them.
                return Forbid();
            }

            // 4. If the check passes, delete the rating.
            _unitOfWork.rating.Delete(rating);
            await _unitOfWork.SaveAsync();

            // 5. Return a 204 No Content response, which is the standard for a successful DELETE.
            return NoContent();
        }

        [HttpPost]
        [Authorize] // A user must be logged in to create a rating.
        public async Task<IActionResult> Create([FromBody] RatingDto dto)
        {
            // 1. Basic Model Validation (e.g., are stars between 1 and 5?)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Get the authenticated user's ID from their token/cookie.
            // NEVER trust a UserId sent from the client.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // This case should be rare due to the [Authorize] attribute, but it's a good safeguard.
                return Unauthorized("User ID claim not found in token.");
            }

            // 3. Business Rule: Check if the user has already rated this recipe.
            var existingRating = await _unitOfWork.rating
                .FindAsync(r => r.RecipeId == dto.RecipeId && r.UserId == userId);

            if (existingRating.Any())
            {
                return Conflict("You have already submitted a rating for this recipe.");
            }

            // 4. Create the new database entity from the DTO.
            var newRating = new Rating
            {
                Stars = dto.Stars,
                Comment = dto.Comment,
                RecipeId = dto.RecipeId,
                UserId = userId, // Set the UserId from the authenticated user.
                commentAt = DateTime.UtcNow
            };

            // 5. Save the new rating to the database.
            await _unitOfWork.rating.AddAsync(newRating);
            await _unitOfWork.SaveAsync();

            // 6. Return a proper RESTful response.
            // Fetch the newly created rating again, this time with its navigation properties (User, Recipe)
            // so we can return the complete object to the client.
            var createdRatingWithDetails = await _unitOfWork.rating.GetByIdWithIncludeAsync(newRating.Id, r => r.User, r => r.Recipe);

            // Use CreatedAtAction to return a 201 Created status code.
            // This also adds a "Location" header to the response, pointing to the new resource's URL.
            return CreatedAtAction(nameof(GetById), new { id = newRating.Id }, ToDto(createdRatingWithDetails));
        }
    }
}
