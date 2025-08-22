using FoodSecrets.Models;
using RecipeCorner.Dtos;

public interface IAuthService
{
    Task<RegisterResponseDto?> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);
}