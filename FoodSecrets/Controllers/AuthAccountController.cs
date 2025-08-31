using FoodSecrets.Models;
using FoodSecrets.Services;
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


        // ✅ CORRECTED: More robust Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(); // Call API to invalidate refresh token
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "AuthAccount");
        }

        // In FoodSecrets/Controllers/AuthAccountController.cs

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            // This is the correct, reliable, and efficient way to get user details for the view.
            // It reads directly from the user's authenticated claims cookie, which is the
            // "source of truth" for the current session and avoids potential race conditions
            // from re-fetching data from the API immediately after an update.
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
        public async Task<IActionResult> Settings(IFormFile? ProfileImage) // Note: We only need the file from the form directly
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // STEP 1: Fetch the user's current data to create a valid base model.
            // This pre-populates the model with data that isn't on the form, like the Email.
            var modelToUpdate = new UpdateProfile
            {
                Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                ProfileImageUrl = User.FindFirstValue("ProfileImageUrl") ?? "/images/student-avatar.jpg"
            };

            // STEP 2: Explicitly bind ONLY the editable form fields to your model.
            // This prevents validation errors on fields like 'Email' that aren't being changed.
            // 'TryUpdateModelAsync' is perfect for this. It updates 'modelToUpdate.FullName' from the form.
            if (!await TryUpdateModelAsync(modelToUpdate, "", m => m.FullName))
            {
                // If binding or validation of FullName fails, return the view.
                // The model is already correctly populated with the original data.
                return View(modelToUpdate);
            }

            // If we reach here, ModelState is VALID for the fields we updated.

            string? newImagePath = null;

            // STEP 3: Handle the file upload (your existing logic is good).
            if (ProfileImage != null)
            {
                var oldImagePath = modelToUpdate.ProfileImageUrl; // Use the path from our model
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{Guid.NewGuid()}_{Path.GetExtension(ProfileImage.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                newImagePath = $"/images/users/{fileName}";

                // Delete old image
                if (!string.IsNullOrEmpty(oldImagePath) && !oldImagePath.Contains("student-avatar.jpg"))
                {
                    var oldFullPath = Path.Combine(_env.WebRootPath, oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFullPath))
                    {
                        System.IO.File.Delete(oldFullPath);
                    }
                }
            }

            // STEP 4: Prepare the final object for the API call.
            modelToUpdate.ProfileImageUrl = newImagePath; // This will be null if no new image was uploaded

            // STEP 5: Call the service with the fully valid and updated model.
            var updateResult = await _authService.UpdateProfileAsync(userId, modelToUpdate);

            if (updateResult == null || updateResult.Token == null)
            {
                ModelState.AddModelError("", "Failed to update profile. Please try again.");
                return View(modelToUpdate); // Return the model we've been working with
            }

            // STEP 6: Ensure the response object has the new image path before refreshing the cookie.
            if (!string.IsNullOrEmpty(newImagePath))
            {
                updateResult.ProfileImageUrl = newImagePath;
            }

            // STEP 7: Re-authenticate to update claims in the cookie.
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
