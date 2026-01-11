using System.Linq.Expressions;

namespace ECommerce.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        void Update(T entity);
        void Delete(T entity);

        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);

        Task<int> CountAsync();
        Task<IReadOnlyList<T>> GetPagedAsync(int pageNumber, int pageSize);
    }
}