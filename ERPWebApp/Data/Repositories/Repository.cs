using ERPWebApp.Data.Repositories.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;

namespace ERPWebApp.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }

    [Obsolete("WARNING: Use this method only if it is necessary to get ALL data. Otherwise, use GetListByFilter() for better performance.")]
    public List<T> GetAll(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return query.ToList();
    }

    public T GetById(int id)
    {
        return _context.Set<T>().Find(id);
    }
    public bool Any(Expression<Func<T, bool>> expression = null)
    {
        return _context.Set<T>().Any(expression);
    }

    public List<T> Find(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression).ToList();
    }

    public T FilterOne(Expression<Func<T, bool>> expression = null, Expression<Func<T, object>>[] includes = null)
    {
        IQueryable<T> query = _context.Set<T>();
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (expression != null)
        {
            return query.FirstOrDefault(expression);
        }
        else
        {
            return query.FirstOrDefault();
        }
    }

    public T Add(T entity)
    {
        _context.Set<T>().Add(entity);
        return entity;
    }
    public List<T> AddRange(List<T> entities)
    {
        _context.Set<T>().AddRangeAsync(entities);
        return entities;
    }

    public T Update(T entity)
    {
        _context.Update(entity);
        return entity;
    }

    public void UpdateRange(List<T> entities)
    {
        _context.UpdateRange(entities);
    }

    public T Delete(int id)
    {
        var entity = _context.Set<T>().Find(id);
        if (entity == null)
        {
            return entity;
        }

        _context.Set<T>().Remove(entity);
        return entity;
    }

    public List<T> RemoveRange(List<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
        return entities;
    }

    public List<T> GetListByFilter(
        Expression<Func<T, bool>>[] predicates,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        IQueryable<T> query = _context.Set<T>();
        if (predicates != null)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return query.ToList();
    }
    public List<T> GetListByFilter(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        IQueryable<T> query = _context.Set<T>().Where(expression);

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return query.ToList();
    }

    public bool IsExists(Expression<Func<T, bool>> expression = null)
    {
        if (expression != null)
        {
            return _context.Set<T>().Any(expression);
        }
        else
        {
            return _context.Set<T>().Any();
        }
    }
    public IQueryable<TResult> QueryFilter<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return query(_context.Set<T>());
    }

    #region Async methods
    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        IQueryable<T> query = _context.Set<T>();

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().Where(expression).ToListAsync();
    }

    public async Task<T> FilterOneAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (expression != null)
        {
            return await query.FirstOrDefaultAsync(expression);
        }

        return await query.FirstOrDefaultAsync();
    }
    public async Task<T> FilterOneAsNoTrackingAsync(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _context.Set<T>();
        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (expression != null)
        {
            return await query.AsNoTracking().FirstOrDefaultAsync(expression);
        }

        return await query.AsNoTracking().FirstOrDefaultAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }
    public async Task<List<T>> AddRangeAsync(List<T> entities)
    {
        await _context.Set<T>().AddRangeAsync(entities);
        return entities;
    }

    public async Task<T> DeleteAsync(int id)
    {
        var entity = await _context.Set<T>().FindAsync(id);
        if (entity == null)
        {
            return entity;
        }

        _context.Set<T>().Remove(entity);
        return entity;
    }

    public async Task RemoveRangeAsync(IEnumerable<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task<List<T>> GetListByFilterAsync(
        Expression<Func<T, bool>>[] predicates,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        IQueryable<T> query = _context.Set<T>();
        if (predicates != null)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<List<T>> GetListByFilterAsync(
         Expression<Func<T, bool>> expression,
         Expression<Func<T, string>>[] orderSelectors = null,
         Expression<Func<T, object>>[] includes = null
     )
    {
        IQueryable<T> query = _context.Set<T>().Where(expression);

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (orderSelectors != null)
        {
            foreach (var selector in orderSelectors)
            {
                query = query.OrderBy(selector);
            }
        }

        return await query.ToListAsync();
    }

    public async Task<List<TResult>> GetListByQueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return await query(_context.Set<T>()).ToListAsync();
    }

    public async Task<TResult> GetByQueryAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return await query(_context.Set<T>()).FirstOrDefaultAsync();
    }
    #endregion Async Methods

    public DbDataReader ExecuteStoredProcedure(DbConnection conn, string procedure, int timeout, SqlParameter[] parameters = null)
    {
        try
        {
            using var command = conn.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = procedure;
            command.CommandTimeout = timeout;

            if (parameters != null && parameters.Any())
            {
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter != null)
                        command.Parameters.Add(parameter);
                }
            }
            return command.ExecuteReader();

        }
        catch (Exception)
        {
            throw;
        }
    }

    public int GetCount(Expression<Func<T, bool>> expression = null)
    {
        if(expression != null) return _context.Set<T>().Count(expression);
        return _context.Set<T>().Count();
    }

    public async Task<int> GetCountAsync(Expression<Func<T, bool>> expression = null)
    {
        if(expression != null) return await _context.Set<T>().CountAsync(expression);
        return  await _context.Set<T>().CountAsync();
    }

    public List<TResult> GetListByQuery<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return query(_context.Set<T>()).ToList();
    }

    public TResult GetByQuery<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
       return query(_context.Set<T>()).FirstOrDefault();
    }
    public async Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression = null)
    {
        if (expression != null)
        {
            return await _context.Set<T>().AnyAsync(expression);
        }
        else
        {
            return await _context.Set<T>().AnyAsync();
        }
    }
}