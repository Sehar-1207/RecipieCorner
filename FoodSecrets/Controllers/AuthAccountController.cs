using FoodSecrets.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Dtos;
using System.Data;
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
            // ✅ Role-based redirect
            if (result.Roles != null && result.Roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "RecipeUi");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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

            // Extract roles directly from JWT
            var roles = ExtractRolesFromJwt(result.Token.AccessToken);

            await HandleSuccessfulLogin(result);
            TempData["Message"] = $"Welcome back, {result.FullName}!";

            // ✅ Role-based redirect using roles list
            if (roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "RecipeUi"); // Admin dashboard
            }
            else
            {
                return RedirectToAction("Index", "Home"); // User dashboard
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(); 
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "AuthAccount");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            var model = new UpdateProfile
            {
                FullName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                ProfileImageUrl = User.FindFirstValue("ProfileImageUrl") ?? "/images/student-avatar.jpg"
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(IFormFile? ProfileImage) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var modelToUpdate = new UpdateProfile
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                ProfileImageUrl = User.FindFirstValue("ProfileImageUrl") ?? "/images/student-avatar.jpg"
            };

            if (!await TryUpdateModelAsync(modelToUpdate, "", m => m.FullName))
            {
                return View(modelToUpdate);
            }

            // If we reach here, ModelState is VALID for the fields we updated.

            string? newImagePath = null;

          
            if (ProfileImage != null)
            {
                var oldImagePath = modelToUpdate.ProfileImageUrl; 
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetExtension(ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                newImagePath = $"/images/users/{fileName}";

                if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("student-avatar.jpg"))
                {
                    var oldFullPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFullPath))
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }
            }

            modelToUpdate.ProfileImageUrl = newImagePath; 

            var updateResult = await _authService.UpdateProfileAsync(userId, modelToUpdate);

            if (updateResult == null || updateResult.Token == null)
            {
                ModelState.AddModelError("", "Failed to update profile. Please try again.");
                return View(modelToUpdate); 
            }

            if (!string.IsNullOrEmpty(newImagePath))
            {
                updateResult.ProfileImageUrl = newImagePath;
            }

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
