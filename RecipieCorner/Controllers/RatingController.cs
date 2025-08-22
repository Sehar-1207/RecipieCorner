using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: api/rating
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ratings = await _unitOfWork.rating
                .GetAllWithIncludeAsync(r => r.User);   // make sure User is included

            var dtos = ratings.Select(r => new RatingDto
            {
                Id = r.Id,
                Stars = r.Stars,
                Comment = r.Comment,
                RecipeId = r.RecipeId,
                UserId = r.UserId,
                UserName = r.User?.UserName,
                ProfileImageUrl = r.User?.ProfileImageUrl,
                commentAt = r.commentAt
            });

            return Ok(dtos);
        }

        // GET: api/rating/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rating = await _unitOfWork.rating
                .GetByIdWithIncludeAsync(id, r => r.User);

            if (rating == null) return NotFound();

            var dto = new RatingDto
            {
                Id = rating.Id,
                Stars = rating.Stars,
                Comment = rating.Comment,
                RecipeId = rating.RecipeId,
                UserId = rating.UserId,
                UserName = rating.User?.UserName,
                ProfileImageUrl = rating.User?.ProfileImageUrl,
                commentAt = rating.commentAt
            };

            return Ok(dto);
        }

        // POST: api/rating
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RatingDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // get current logged in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return Unauthorized();

            var rating = new Rating
            {
                Stars = dto.Stars,
                Comment = dto.Comment,
                RecipeId = dto.RecipeId,
                UserId = user.Id,
                commentAt = DateTime.UtcNow
            };

            await _unitOfWork.rating.AddAsync(rating);
            await _unitOfWork.SaveAsync();

            var resultDto = new RatingDto
            {
                Id = rating.Id,
                Stars = rating.Stars,
                Comment = rating.Comment,
                RecipeId = rating.RecipeId,
                UserId = user.Id,
                UserName = user.UserName,
                ProfileImageUrl = user.ProfileImageUrl,
                commentAt = rating.commentAt
            };

            return CreatedAtAction(nameof(GetById), new { id = rating.Id }, resultDto);
        }

        // PUT: api/rating/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RatingDto dto)
        {
            if (id != dto.Id) return BadRequest("ID mismatch");

            var existing = await _unitOfWork.rating.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // ensure only owner can edit
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existing.UserId != userId) return Forbid();

            existing.Stars = dto.Stars;
            existing.Comment = dto.Comment;

            _unitOfWork.rating.Update(existing);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        // DELETE: api/rating/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _unitOfWork.rating.GetByIdAsync(id);
            if (rating == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (rating.UserId != userId) return Forbid();

            _unitOfWork.rating.Delete(rating);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}
