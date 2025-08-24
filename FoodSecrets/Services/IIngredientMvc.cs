namespace FoodSecrets.Services
{
    public interface IIngredientMvc
    {
         Task<IEnumerable<IngredientDto>> GetAllAsync();
        Task<IngredientDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(IngredientDto dto);
        Task<bool> UpdateAsync(int id, IngredientDto dto);
        Task<bool> DeleteAsync(int id);
    }
}