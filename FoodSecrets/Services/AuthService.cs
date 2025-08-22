using FoodSecrets.Models;
using RecipeCorner.Dtos;
using System.Text.Json;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;

    public AuthService(HttpClient http)
    {
        _http = http;
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

        var response = await _http.PostAsync("api/auth/register", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<RegisterResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var response = await _http.PostAsJsonAsync("api/auth/refresh", refreshToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<LoginResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return wrapper?.Token;
    }
}
