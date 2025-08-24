using FoodSecrets.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RecipeCorner.Dtos;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        // ✅ GET: /AuthAccount/Register
        public IActionResult Register() => View();

        // ✅ POST: /AuthAccount/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Save profile image if provided
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "users");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                dto.ProfileImage = "/images/users/" + fileName;
            }

            var result = await _authService.RegisterAsync(dto);

            if (result == null || result.Token == null || string.IsNullOrEmpty(result.Token.AccessToken))
            {
                ModelState.AddModelError("", "Registration failed.");
                return View(dto);
            }

            // ✅ Extract roles from JWT
            var roles = ExtractRolesFromJwt(result.Token.AccessToken);

            SaveSession(result.FullName, result.Token.AccessToken, result.Token.RefreshToken, result.ProfileImageUrl, roles);
            await SignInWithCookie(result.FullName, result.Token.AccessToken, roles);

            TempData["Message"] = "Registration successful!";
            return RedirectToAction("Index", "RecipeUi");
        }

        // ✅ GET: /AuthAccount/Login
        public IActionResult Login() => View();

        // ✅ POST: /AuthAccount/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            try
            {
                var result = await _authService.LoginAsync(dto);

                if (result == null || result.Token == null || string.IsNullOrEmpty(result.Token.AccessToken))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(dto);
                }

                // ✅ Extract roles from JWT
                var roles = ExtractRolesFromJwt(result.Token.AccessToken);

                SaveSession(result.FullName, result.Token.AccessToken, result.Token.RefreshToken, result.ProfileImageUrl, roles);
                await SignInWithCookie(result.FullName, result.Token.AccessToken, roles);

                TempData["Message"] = $"Welcome back, {result.FullName}!";
                return RedirectToAction("Index", "RecipeUi");
            }
            catch
            {
                ModelState.AddModelError("", "Login failed. Please try again.");
                return View(dto);
            }
        }

        // ✅ Logout
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "AuthAccount");
        }

        // 🔹 Helper: Save session
        private void SaveSession(string? fullName, string accessToken, string? refreshToken, string? profileImageUrl, List<string>? roles)
        {
            HttpContext.Session.SetString("AccessToken", accessToken);
            HttpContext.Session.SetString("RefreshToken", refreshToken ?? "");
            HttpContext.Session.SetString("UserName", fullName ?? "Guest");
            HttpContext.Session.SetString("UserImage", profileImageUrl ?? "/images/default.png");

            if (roles != null && roles.Count > 0)
            {
                HttpContext.Session.SetString("UserRoles", string.Join(",", roles));
            }
        }

        // 🔹 Helper: Sign in with cookie (with roles)
        private async Task SignInWithCookie(string? userName, string accessToken, List<string>? roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName ?? "Guest"),
                new Claim("AccessToken", accessToken)
            };

            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });
        }

        // 🔹 Helper: Extract roles from JWT token
        private List<string> ExtractRolesFromJwt(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Roles might be "role" or ClaimTypes.Role depending on API
            var roles = jwtToken.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToList();

            return roles;
        }
    }
}
