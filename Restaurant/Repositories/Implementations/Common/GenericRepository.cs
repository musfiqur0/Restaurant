using Microsoft.EntityFrameworkCore;
using Restaurant.DataAccessLayer;
using System.Linq.Expressions;

namespace Restaurant.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly DbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(RestaurantDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = _dbContext.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        try
        {
            return _dbSet.AsQueryable();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<T> GetByIdAsync(object id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<T> InsertAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            return entity; // return inserted entity
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task<T> UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
            return Task.FromResult(entity); // return updated entity
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task DeleteAsync(object id)
    {
        try
        {
            var entityToDelete = await _dbSet.FindAsync(id);
            if (entityToDelete != null)
                _dbSet.Remove(entityToDelete);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<int> SaveAsync()
    {
        try
        {
            return await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<bool> IsExistsAsync(object id)
    {
        return await _dbSet.FindAsync(id) != null;
    }
    public async Task<long> GetNextIdAsync(string propertyName)
    {
        try
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property == null)
            {
                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");
            }

            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyExpression = Expression.Convert(Expression.Property(parameter, property), typeof(long?));
            var lambda = Expression.Lambda<Func<T, long?>>(propertyExpression, parameter);

            var maxId = await _dbSet.MaxAsync(lambda);
            return maxId.HasValue ? maxId.Value + 1 : 1;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
