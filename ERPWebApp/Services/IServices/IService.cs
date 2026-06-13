using System.Linq.Expressions;

namespace ERPWebApp.Services.IServices;

public interface IService<T> where T : class
{
    T Get(Expression<Func<T, bool>> expression = null, Expression<Func<T, object>>[] includes = null);
    Task<T> GetAsync(Expression<Func<T, bool>> expression, Expression<Func<T, object>>[] includes = null);
    Task<T> GetAsNoTrackingAsync(
        Expression<Func<T, bool>> expression = null,
        Expression<Func<T, object>>[] includes = null
    );
    bool Any(Expression<Func<T, bool>> expression = null);
    List<T> GetList(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    List<T> GetAll(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    Task<List<T>> GetAllAsync(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    IQueryable<TResult> QueryFilter<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    T Add(T entity);

    /// <summary>
    /// Creates a new entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<T> AddAsync(T entity);
    bool IsExists(Expression<Func<T, bool>> expression = null);
    Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression = null);
    T Update(T entity);

    /// <summary>
    /// Updates an entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to be updated.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<int> UpdateAsync(T entity);
    int Remove(int id);

    /// <summary>
    /// Deletes an entity asynchronously by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to be deleted.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<int> RemoveAsync(int id);
    int GetCount(Expression<Func<T, bool>> expression);
    Task<int> GetCountAsync(Expression<Func<T, bool>> expression = null);
    List<TResult> GetList<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    Task<List<TResult>> GetListAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    TResult Get<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    Task<TResult> GetAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
}