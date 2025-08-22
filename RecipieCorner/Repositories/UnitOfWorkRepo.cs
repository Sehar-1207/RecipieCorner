using RecipeCorner.Data;
using RecipeCorner.Interfaces;
using RecipeCorner.Models;

namespace RecipeCorner.Repositories
{
    public class UnitOfWorkRepo: IUnitOfWork
        {
            private readonly ApplicationDbContext _context;

            public IGeneric<Recipe> recipes { get; }
            public IGeneric<Ingredient> ingredients { get; }
            public IGeneric<Instruction> instructions { get; }
            public IGeneric<Rating> rating { get; }

            public UnitOfWorkRepo(ApplicationDbContext context)
            {
                _context = context;
                recipes = new GenericRepo<Recipe>(_context);
                ingredients = new GenericRepo<Ingredient>(_context);
                instructions = new GenericRepo<Instruction>(_context);
                rating = new GenericRepo<Rating>(_context);
            }

            public async Task<int> SaveAsync()
            {
                return await _context.SaveChangesAsync();
            }
        
    }
}
