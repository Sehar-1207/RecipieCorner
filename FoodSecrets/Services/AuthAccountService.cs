using FoodSecrets.Models;
using RecipeCorner.Dtos;
using System.Net.Http.Headers;
using System.Security.Claims; // ✅ ADDED: For accessing claims
using System.Text;
using System.Text.Json;

namespace FoodSecrets.Services
{
    public class AuthAccountService : IAuthAccountService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthAccountService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, string? profileImageUrl)
        {
            dto.ProfileImage = profileImageUrl;

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/register", content);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOptions);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/login", content);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOptions);
        }

        public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
        {
            AddJwtHeader(); // This method is now fixed to be reliable

            var response = await _httpClient.GetAsync($"api/Auth/me");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserDetailsDto>(json, _jsonOptions);
        }

        public async Task<AuthResponseDto?> UpdateProfileAsync(string userId, UpdateProfile dto)
        {
            AddJwtHeader();

            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Auth/update-profile", content);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOptions);
        }

        // 🔹 Refresh Token
        public async Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var content = new StringContent(JsonSerializer.Serialize(refreshToken), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/refresh", content);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            // Deserialization happens here, the HandleSuccessfulLogin method will update session/cookie
            return JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOptions);
        }

        // 🔹 Logout
        public async Task<bool> LogoutAsync()
        {
            AddJwtHeader(); // Add auth token to invalidate the refresh token on the API side

            var response = await _httpClient.PostAsync("api/Auth/logout", null);

            // Clearing local session/cookie is handled by the controller's SignOutAsync
            return response.IsSuccessStatusCode;
        }

        // ✅ CHANGED: This is the key fix.
        // It now gets the token from the user's claims principal (stored in the cookie),
        // which is far more reliable than the session state.
        private void AddJwtHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null; // Clear previous headers
            var token = _httpContextAccessor.HttpContext?
                                           .User.Claims
                                           .FirstOrDefault(c => c.Type == "AccessToken")?
                                           .Value;

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}