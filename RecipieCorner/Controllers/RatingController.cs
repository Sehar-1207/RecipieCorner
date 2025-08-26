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
            var rating = await _unitOfWork.rating.GetByIdWithIncludeAsync(id, r => r.User);
            if (rating == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (rating.UserId != userId) return Forbid();

            _unitOfWork.rating.Delete(rating);
            await _unitOfWork.SaveAsync();

            return Ok(ToDto(rating));
        }
    }
}
