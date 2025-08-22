using RecipeCorner.Models;

namespace RecipeCorner.Interfaces
{
    public interface IUnitOfWork
    {
        IGeneric<Recipe> recipes { get; }
        IGeneric<Ingredient> ingredients { get; }
        IGeneric<Instruction> instructions { get; }
        IGeneric<Rating> rating { get; }
        Task<int> SaveAsync();
    }
}
