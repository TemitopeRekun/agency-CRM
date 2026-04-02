namespace Crm.Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> AsQueryable();
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}
