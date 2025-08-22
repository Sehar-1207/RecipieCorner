namespace FoodSecrets.Services
{
    public interface IRecipe
    {
        Task<IEnumerable<RecipeDto>> GetAllAsync();
        Task<RecipeDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(RecipeDto recipe);
        Task<bool> UpdateAsync(int id, RecipeDto recipe);
        Task<bool> DeleteAsync(int id);
    }
}
