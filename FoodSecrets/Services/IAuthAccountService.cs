using FoodSecrets.Models;
using RecipeCorner.Dtos;

public interface IAuthAccountService
{
    // Register: profile image is passed as string path
    Task<AuthResponseDto?> RegisterAsync(RegisterDto dto, string? profileImagePath);

    Task<AuthResponseDto?> LoginAsync(LoginDto dto);

    Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken);

    // UpdateProfile: only full name and image path string
    Task<AuthResponseDto?> UpdateProfileAsync(string userId, UpdateProfile dto);

    Task<UserDetailsDto?> GetUserDetailsAsync(string userId);

    void Logout();
}
