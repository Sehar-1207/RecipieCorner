using FoodSecrets.Models;
using RecipeCorner.Dtos;

public interface IAuthAccountService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, string? profileImageUrl);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserDetailsDto?> GetUserDetailsAsync(string userId);
    Task<AuthResponseDto?> UpdateProfileAsync(string userId, UpdateProfile dto);
    Task<AuthResponseDto?> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync();
}
