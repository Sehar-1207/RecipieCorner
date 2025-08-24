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
        private readonly object _signInManager;

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
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // ✅ Assign Role before CreateAsync
            var roleToAssign = "User"; // default
            var adminSecret = _config["AdminSecretCode"];
            if (!string.IsNullOrEmpty(dto.SecretKey) && dto.SecretKey.Trim() == adminSecret?.Trim())
            {
                roleToAssign = "Admin";
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                ProfileImageUrl = dto.ProfileImage,
                Role = roleToAssign  // ✅ Role set here
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // ✅ Ensure roles exist in Identity tables
            await EnsureRolesExistAsync("User", "Admin");

            // ✅ Add role to Identity system
            await _userManager.AddToRoleAsync(user, roleToAssign);

            // JWT + Refresh Token
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
                user.ProfileImageUrl,
                user.Role   // ✅ Return role to client
            });
        }



        // In RecipeCorner.Controllers.AuthController.Login
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

            // ✅ get first role (or join multiple if you support that)
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? user.Role ?? "User";

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
                user.ProfileImageUrl,
                Role = role                    // ✅ include role
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
