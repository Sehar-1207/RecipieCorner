using Microsoft.AspNetCore.Mvc;
using RecipeCorner.Dtos;

namespace FoodSecrets.Controllers
{
    public class AuthAccountController : Controller
    {
        private readonly IAuthAccountService _authService;

        public AuthAccountController(IAuthAccountService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.RegisterAsync(dto);
            if (result == null)
            {
                ModelState.AddModelError("", "Registration failed");
                return View(dto);
            }

            // Optionally save tokens in session
            HttpContext.Session.SetString("AccessToken", result.Token.AccessToken);
            HttpContext.Session.SetString("RefreshToken", result.Token.RefreshToken);

            TempData["Message"] = "Registration successful!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Call AuthService → API validates email & password
            var result = await _authService.LoginAsync(dto);
            if (result == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(dto);
            }

            // Save tokens in session
            HttpContext.Session.SetString("AccessToken", result.Token.AccessToken);
            HttpContext.Session.SetString("RefreshToken", result.Token.RefreshToken);

            TempData["Message"] = $"Welcome, {result.FullName}!";
            return RedirectToAction("Index", "Home");
        }


        // Optional: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AccessToken");
            HttpContext.Session.Remove("RefreshToken");
            return RedirectToAction("Login");
        }
    }
}
