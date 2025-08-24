using RecipeCorner.Interfaces;
using RecipeCorner.Models;
using System.Threading.Tasks;

namespace FoodSecrets.Services
{
    public interface IRecipeMvc
    {
        Task<IEnumerable<RecipeDto>> GetAllAsync();
        Task<RecipeDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(RecipeDto recipe);
        Task<bool> UpdateAsync(int id, RecipeDto recipe);
        Task<bool> DeleteAsync(int id);
        Task<RecipeDetailsDto?> GetDetailsAsync(int id);

        Task<IEnumerable<RecipeDto>> GetByCuisineAsync(string cuisine);
    }

}

