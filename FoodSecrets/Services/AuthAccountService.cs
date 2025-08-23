using FoodSecrets.Models;
using RecipeCorner.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

public class AuthAccountService : IAuthAccountService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthAccountService(HttpClient http)
    {
        _http = http;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    public async Task<RegisterResponseDto?> RegisterAsync(RegisterDto dto)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(dto.FullName), "FullName");
        content.Add(new StringContent(dto.Email), "Email");
        content.Add(new StringContent(dto.Password), "Password");

        if (!string.IsNullOrEmpty(dto.SecretKey))
            content.Add(new StringContent(dto.SecretKey), "SecretKey");

        if (dto.ProfileImage != null)
        {
            var stream = dto.ProfileImage.OpenReadStream();
            content.Add(new StreamContent(stream), "ProfileImage", dto.ProfileImage.FileName);
        }

        var response = await _http.PostAsync("api/Auth/register", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Register failed: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RegisterResponseDto>(json, _jsonOptions);
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/login", dto);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponseDto>(json, _jsonOptions);
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/Auth/refresh", new { RefreshToken = refreshToken });

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Refresh token failed: {response.StatusCode} - {error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<LoginResponseDto>(json, _jsonOptions);
        return wrapper?.Token;
    }
}
