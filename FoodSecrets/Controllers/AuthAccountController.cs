using FoodSecrets.Models;
using FoodSecrets.Services;
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
        [AllowAnonymous] // This ensures anyone can see the access denied page
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid) return View(dto);

            string? imagePath = null;

            if (ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
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

        // ✅ CORRECTED: More robust Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(); // Call API to invalidate refresh token
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "AuthAccount");
        }

        [Authorize] // Ensures only logged-in users can see this
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "AuthAccount");
            }

            var currentUser = await _authService.GetUserDetailsAsync(userId);
            if (currentUser == null)
            {
                // This could happen if the API is down or token is invalid.
                // Log them out to be safe.
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "AuthAccount");
            }

            var model = new UpdateProfile
            {
                FullName = currentUser.FullName ?? string.Empty,
                Email = currentUser.Email ?? string.Empty,
                // Use a default image if none is set
                ProfileImageUrl = !string.IsNullOrEmpty(currentUser.ProfileImageUrl)
                                    ? currentUser.ProfileImageUrl
                                    : "/images/student-avatar.jpg"
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(UpdateProfile dto, IFormFile? ProfileImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // We must re-populate the ProfileImageUrl for the view if validation fails
            if (!ModelState.IsValid)
            {
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                dto.ProfileImageUrl = currentUser?.ProfileImageUrl ?? "/images/student-avatar.jpg";
                return View(dto);
            }

            string? newImagePath = null;

            // Handle new profile image upload
            if (ProfileImage != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{ProfileImage.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }
                newImagePath = $"/images/users/{fileName}";

                // Delete old image if it exists and is not the default one
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                if (!string.IsNullOrEmpty(currentUser?.ProfileImageUrl) && !currentUser.ProfileImageUrl.Contains("student-avatar.jpg"))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, currentUser.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }

            // Set the image URL in the DTO to be sent to the API
            // If a new image was uploaded, use its path. Otherwise, this will be null,
            // and the API will know not to update the image URL.
            dto.ProfileImageUrl = newImagePath;

            var updateResult = await _authService.UpdateProfileAsync(userId, dto);

            if (updateResult == null || updateResult.Token == null)
            {
                ModelState.AddModelError("", "Failed to update profile. Please try again.");
                var currentUser = await _authService.GetUserDetailsAsync(userId);
                dto.ProfileImageUrl = currentUser?.ProfileImageUrl ?? "/images/student-avatar.jpg";
                return View(dto);
            }

            // Re-authenticate with the new token and claims
            await HandleSuccessfulLogin(updateResult);
            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Settings");
        }

        private async Task HandleSuccessfulLogin(AuthResponseDto result)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(result.Token.AccessToken);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
            var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            // ✅ Save to Cookie Claims (The MOST IMPORTANT part for reliable auth)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, result.FullName ?? "Guest"),
                new Claim(ClaimTypes.Email, result.Email ?? ""),
                new Claim("AccessToken", result.Token.AccessToken), // Critical for API calls
                new Claim("RefreshToken", result.Token.RefreshToken ?? ""),
                new Claim("ProfileImageUrl", result.ProfileImageUrl ?? "/images/default.png")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Remember me
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            // ✅ Save to Session (Useful for quick access in UI layouts, but not for auth)
            HttpContext.Session.SetString("AccessToken", result.Token.AccessToken);
            HttpContext.Session.SetString("FullName", result.FullName ?? "Guest");
            HttpContext.Session.SetString("ProfileImageUrl", result.ProfileImageUrl ?? "/images/default.png");
        }


        private List<string> ExtractRolesFromJwt(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

            return jwtToken.Claims
                           .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                           .Select(c => c.Value)
                           .ToList();
        }

        private string ExtractUserIdFromJwt(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtToken.Claims
                           .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
                           ?.Value ?? "";
        }

        private async Task<bool> TryRefreshTokenAsync()
        {
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (result == null || result.Token?.AccessToken == null) return false;

            await HandleSuccessfulLogin(result); // update session + claims
            return true;
        }

    }
}
