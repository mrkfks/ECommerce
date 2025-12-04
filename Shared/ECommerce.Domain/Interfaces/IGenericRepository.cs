using System.Linq.Expressions;

namespace ECommerce.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        //READ
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        //CREATE
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        //UPDATE
        void Update(T entity);

        //DELETE
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}