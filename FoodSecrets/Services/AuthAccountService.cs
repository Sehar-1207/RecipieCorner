using FoodSecrets.Models;
using Microsoft.AspNetCore.Http;
using RecipeCorner.Dtos;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

public class AuthAccountService : IAuthAccountService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly IHttpContextAccessor _httpContext;

    public AuthAccountService(IHttpClientFactory httpClientFactory, IConfiguration config, IHttpContextAccessor httpContext)
    {
        _http = httpClientFactory.CreateClient();
        _http.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _httpContext = httpContext;
    }

    #region Public Authentication Methods

    public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, string? profileImagePath)
    {
        var payload = new
        {
            dto.FullName,
            dto.Email,
            dto.Password,
            ProfileImageUrl = profileImagePath // just the string path
        };

        var response = await _http.PostAsJsonAsync("api/Auth/register", payload);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        if (result?.Token != null) SaveTokens(result.Token);

        return result;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        if (result?.Token != null) SaveTokens(result.Token);

        return result;
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(string userId)
    {
        AddAuthorizationHeader();
        var response = await _http.GetAsync("api/Auth/me");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<UserDetailsDto>(_jsonOptions);
    }

    //public async Task<AuthResponseDto?> UpdateProfileAsync(string userId, UpdateProfile dto)
    //{
    //    AddAuthorizationHeader();

    //    // Only send string paths to backend
    //    var payload = new
    //    {
    //        dto.FullName,
    //        ProfileImageUrl = dto.ProfileImageUrl
    //    };

    //    var response = await _http.PostAsJsonAsync("api/Auth/update-profile", payload);
    //    if (!response.IsSuccessStatusCode) return null;

    //    var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
    //    if (result?.Token != null) SaveTokens(result.Token);

    //    return result;
    //}
    public async Task<AuthResponseDto?> UpdateProfileAsync(string userId, UpdateProfile dto)
    {
        AddAuthorizationHeader();

        // Create simple JSON payload with string path
        var response = await _http.PostAsJsonAsync("api/Auth/update-profile", dto);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(_jsonOptions);
        if (result?.Token != null) SaveTokens(result.Token);

        return result;
    }


    public void Logout()
    {
        var session = _httpContext.HttpContext?.Session;
        session?.Remove("AccessToken");
        session?.Remove("RefreshToken");
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/refresh", new { refreshToken });
        if (!response.IsSuccessStatusCode)
        {
            Logout();
            return null;
        }

        var tokenString = await response.Content.ReadAsStringAsync();

        // Assuming your API returns a plain string token, just wrap it
        return new TokenResponseDto
        {
            AccessToken = tokenString,  // or deserialize if JSON
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30) // adjust as needed
        };
    }

    private void SaveTokens(TokenResponseDto token)
    {
        var session = _httpContext.HttpContext?.Session;
        session?.SetString("AccessToken", token.AccessToken);
        session?.SetString("RefreshToken", token.RefreshToken);
    }

    private void AddAuthorizationHeader()
    {
        var token = _httpContext.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #endregion
}
