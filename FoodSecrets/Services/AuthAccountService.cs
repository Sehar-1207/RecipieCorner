using FoodSecrets.Models;
using RecipeCorner.Dtos;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

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

    public async Task<RegisterResponseDto?> RegisterAsync(RegisterDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/register", dto);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Register failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

        var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(_jsonOptions);

        if (result?.Token != null)
            SaveTokens(result.Token);

        return result;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/login", dto);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Login failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);

        if (result?.Token != null)
            SaveTokens(result.Token);

        return result;
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/refresh", new { RefreshToken = refreshToken });

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Refresh token failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

        var wrapper = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);

        if (wrapper?.Token != null)
            SaveTokens(wrapper.Token);

        return wrapper?.Token;
    }

    private void SaveTokens(TokenResponseDto token)
    {
        var session = _httpContext.HttpContext!.Session;
        session.SetString("AccessToken", token.AccessToken);
        session.SetString("RefreshToken", token.RefreshToken);
        session.SetString("ExpiresAt", token.ExpiresAt.ToString("o"));
    }

    public TokenResponseDto? GetSavedTokens()
    {
        var session = _httpContext.HttpContext!.Session;

        var accessToken = session.GetString("AccessToken");
        var refreshToken = session.GetString("RefreshToken");
        var expiresAtStr = session.GetString("ExpiresAt");

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return null;

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.Parse(expiresAtStr!)
        };
    }

    public void Logout()
    {
        var session = _httpContext.HttpContext!.Session;
        session.Remove("AccessToken");
        session.Remove("RefreshToken");
        session.Remove("ExpiresAt");
    }
}
