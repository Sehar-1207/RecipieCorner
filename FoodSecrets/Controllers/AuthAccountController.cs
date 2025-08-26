using FoodSecrets.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FoodSecrets.Controllers
{
    public class AuthAccountController : Controller
    {
        private readonly IAuthAccountService _authService;
        private readonly IWebHostEnvironment _env;

        public AuthAccountController(IAuthAccountService authService, IWebHostEnvironment env)
        {
            _authService = authService;
            _env = env;
        }

        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid) return View(dto);

            string? imagePath = null;

            // Handle image upload
            if (ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await ProfileImage.CopyToAsync(stream);

                imagePath = $"/images/users/{fileName}";
            }

            var result = await _authService.RegisterAsync(dto, imagePath);

            if (result?.Token?.AccessToken == null)
            {
                ModelState.AddModelError("", "Registration failed. Email may already be in use.");
                return View(dto);
            }

            await HandleSuccessfulLogin(result);
            TempData["Message"] = $"Welcome, {result.FullName}! Your account has been created.";
            return RedirectToAction("Index", "RecipeUi");
        }

        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var result = await _authService.LoginAsync(dto);

            if (result?.Token?.AccessToken == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(dto);
            }

            await HandleSuccessfulLogin(result);
            TempData["Message"] = $"Welcome back, {result.FullName}!";
            return RedirectToAction("Index", "RecipeUi");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var userDetails = await _authService.GetUserDetailsAsync(userId);
            if (userDetails == null) return NotFound("User not found.");

            return View(userDetails);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(UpdateProfile dto, IFormFile? ProfileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
            {
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                return View(currentUser);
            }

            string? imagePath = null;

            // Handle new profile image
            if (ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await ProfileImage.CopyToAsync(stream);

                imagePath = $"/images/users/{fileName}";

                // Delete old image
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                if (!string.IsNullOrEmpty(currentUser?.ProfileImageUrl))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, currentUser.ProfileImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }

            dto.ProfileImageUrl = imagePath;

            var updateResult = await _authService.UpdateProfileAsync(userId, dto);

            if (updateResult == null || updateResult.Token == null)
            {
                ModelState.AddModelError("", "Failed to update profile.");
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                return View(currentUser);
            }

            await HandleSuccessfulLogin(updateResult);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Settings");
        }


        private async Task HandleSuccessfulLogin(AuthResponseDto result)
        {
            var userId = ExtractUserIdFromJwt(result.Token.AccessToken);
            var roles = ExtractRolesFromJwt(result.Token.AccessToken);

            HttpContext.Session.SetString("AccessToken", result.Token.AccessToken);
            HttpContext.Session.SetString("RefreshToken", result.Token.RefreshToken);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, result.FullName ?? "Guest"),
                new Claim("AccessToken", result.Token.AccessToken),
                new Claim("RefreshToken", result.Token.RefreshToken ?? ""),
                new Claim("ProfileImageUrl", result.ProfileImageUrl ?? "/images/default.png")
            };

            if (roles != null && roles.Any())
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7),
                    AllowRefresh = true
                });
        }

        private List<string> ExtractRolesFromJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value).ToList();
        }

        private string ExtractUserIdFromJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value ?? "";
        }

    }
}
