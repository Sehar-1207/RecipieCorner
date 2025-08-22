using System.Linq.Expressions;

namespace RecipeCorner.Interfaces
{
    public interface IGeneric<T> where T : class 
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAllWithIncludeAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdWithIncludeAsync(int id, params Expression<Func<T, object>>[] includes);
    }
}
