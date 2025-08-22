using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Dtos;
using RecipeCorner.Models;
using RecipeCorner.Services;

namespace RecipeCorner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwt;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public AuthController(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              JwtTokenService jwt,
                              IConfiguration config,
                              IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwt = jwt;
            _config = config;
            _env = env;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName
            };

            // Handle profile image upload
            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.ProfileImage.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfileImage.CopyToAsync(stream);
                }

                user.ProfileImageUrl = $"/uploads/{fileName}";
            }

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Ensure roles exist
            await EnsureRolesExistAsync("User", "Admin");

            // Assign role
            var adminSecret = _config["AdminSecretCode"];
            if (!string.IsNullOrEmpty(dto.SecretKey) && dto.SecretKey == adminSecret)
                await _userManager.AddToRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "User");

            // Generate tokens
            var accessToken = await _jwt.CreateAccessTokenAsync(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"]!));

            return Ok(new
            {
                Token = new TokenDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                },
                user.FullName,
                user.Email,
                user.ProfileImageUrl
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!valid) return Unauthorized("Invalid credentials");

            var accessToken = await _jwt.CreateAccessTokenAsync(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"]!));

            return Ok(new
            {
                Token = new TokenDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt
                },
                user.FullName,
                user.Email,
                user.ProfileImageUrl
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized("Invalid refresh token");

            var newAccessToken = await _jwt.CreateAccessTokenAsync(user);
            var newRefreshToken = _jwt.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiresMinutes"]!));

            return Ok(new
            {
                Token = new TokenDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = expiresAt
                },
                user.FullName,
                user.Email,
                user.ProfileImageUrl
            });
        }

        private async Task EnsureRolesExistAsync(params string[] roles)
        {
            foreach (var r in roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }
        }
    }
}
