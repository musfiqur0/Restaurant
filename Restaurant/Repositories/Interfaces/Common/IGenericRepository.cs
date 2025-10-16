namespace Restaurant.Repositories;

public interface IGenericRepository<T> where T : class
{
    IQueryable<T> GetAll();
    Task<T> GetByIdAsync(object id);
    Task<T> InsertAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<int> SaveAsync();
    Task<bool> IsExistsAsync(object id);
    Task<long> GetNextIdAsync(string propertyName);
}
