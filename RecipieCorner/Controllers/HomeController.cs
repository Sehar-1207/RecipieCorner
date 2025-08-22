using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RecipeCorner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Get()
        {
            return Ok(new { Message = "Welcome to the Recipe Corner API!" });
        }
    }
}
