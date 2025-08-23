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

            //// ✅ Handle profile image with helper
            //if (dto.ProfileImage != null)
            //{
            //    try
            //    {
            //        user.ProfileImageUrl = await UploadFilet(dto.ProfileImage);
            //    }
            //    catch (Exception ex)
            //    {
            //        return BadRequest(ex.Message);
            //    }
            //}

            var profileImageUrl = dto.ProfileImage != null
    ? await UploadFilet(dto.ProfileImage)
    : null;

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                ProfileImageUrl = profileImageUrl
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // ✅ Ensure roles
            await EnsureRolesExistAsync("User", "Admin");

            // ✅ Assign role
            var adminSecret = _config["AdminSecretCode"];
            if (!string.IsNullOrEmpty(dto.SecretKey) && dto.SecretKey == adminSecret)
                await _userManager.AddToRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "User");

            // ✅ Generate tokens
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


        // ✅ Upload helper function
        private async Task<string> UploadFilet(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only jpg, jpeg, png, gif are allowed.");

            long maxSize = 2 * 1024 * 1024; // 2 MB
            if (file.Length > maxSize)
                throw new ArgumentException("File size cannot exceed 2 MB.");

            // ✅ Make sure WebRootPath is not null
            var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

            var uploadsFolder = Path.Combine(rootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // This will be accessible like: https://localhost:5001/uploads/filename.png
            return $"/uploads/{fileName}";
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
