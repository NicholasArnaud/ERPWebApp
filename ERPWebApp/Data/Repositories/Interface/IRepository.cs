using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Linq.Expressions;

namespace ERPWebApp.Data.Repositories.Interface;

public interface IRepository<T> where T : class
{
    List<T> GetAll(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    bool Any(Expression<Func<T, bool>> expression = null);
    T GetById(int id);
    List<T> Find(Expression<Func<T, bool>> expression);
    T FilterOne(Expression<Func<T, bool>> expression = null, Expression<Func<T, object>>[] includes = null);
    T Add(T entity);
    List<T> AddRange(List<T> entities);
    T Update(T entity);
    void UpdateRange(List<T> entities);
    T Delete(int id);
    List<T> RemoveRange(List<T> enttiies);
    List<T> GetListByFilter(
        Expression<Func<T, bool>>[] predicates,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    List<T> GetListByFilter(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    bool IsExists(Expression<Func<T, bool>> expression = null);
    int GetCount(Expression<Func<T, bool>> expression);
    IQueryable<TResult> QueryFilter<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    List<TResult> GetListByQuery<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    TResult GetByQuery<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);

    #region Async methods
    Task<List<T>> GetAllAsync(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    Task<T> GetByIdAsync(int id);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> expression);
    Task<T> FilterOneAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
    Task<T> FilterOneAsNoTrackingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
    Task<T> AddAsync(T entity);
    Task<List<T>> AddRangeAsync(List<T> entities);
    Task<T> DeleteAsync(int id);
    Task RemoveRangeAsync(IEnumerable<T> entities);
    Task<List<T>> GetListByFilterAsync(
        Expression<Func<T, bool>>[] predicates,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    Task<List<T>> GetListByFilterAsync(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    );
    Task<int> GetCountAsync(Expression<Func<T, bool>> expression);
    Task<List<TResult>> GetListByQueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    Task<TResult> GetByQueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query);
    Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression = null);
    #endregion

    DbDataReader ExecuteStoredProcedure(DbConnection conn, string procedure, int timeout, SqlParameter[] parameters = null);
}