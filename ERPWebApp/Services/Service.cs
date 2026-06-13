using ERPWebApp.Data.Repositories.Interface;
using ERPWebApp.Services.IServices;
using System.Linq.Expressions;

namespace ERPWebApp.Services;

public class Service<T> : IService<T> where T : class
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<T> _repository;

    public Service(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _repository = unitOfWork.GetRepository<T>();
    }

    public T Add(T entity)
    {
        var res = _repository.Add(entity);
        return res;
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var res = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return res;
    }
    public bool Any(Expression<Func<T, bool>> expression = null)
    {
        return _repository.Any(expression);
    }
    public T Get(
        Expression<Func<T, bool>> expression = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return _repository.FilterOne(expression, includes);
    }

    public async Task<T> GetAsync(
        Expression<Func<T, bool>> expression = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return await _repository.FilterOneAsync(expression, includes);
    }

    public async Task<T> GetAsNoTrackingAsync(
        Expression<Func<T, bool>> expression = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return await _repository.FilterOneAsync(expression, includes);
    }

    public List<T> GetAll(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return _repository.GetAll(orderSelectors, includes);
    }

    public async Task<List<T>> GetAllAsync(
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return await _repository.GetAllAsync(orderSelectors, includes);
    }

    public List<T> GetList(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return _repository.GetListByFilter(expression, orderSelectors, includes);
    }

    public async Task<List<T>> GetListAsync(
        Expression<Func<T, bool>> expression,
        Expression<Func<T, string>>[] orderSelectors = null,
        Expression<Func<T, object>>[] includes = null
    )
    {
        return await _repository.GetListByFilterAsync(expression, orderSelectors, includes);
    }

    public bool IsExists(Expression<Func<T, bool>> expression = null)
    {
        return _repository.IsExists(expression);
    }

    public async Task<bool> IsExistsAsync(Expression<Func<T, bool>> expression = null)
    {
        return await _repository.IsExistsAsync(expression);
    }

    public IQueryable<TResult> QueryFilter<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return _repository.QueryFilter(query);
    }
    public int Remove(int id)
    {
        _repository.Delete(id);
        return _unitOfWork.SaveChanges();
    }

    public virtual async Task<int> RemoveAsync(int id)
    {
        await _repository.DeleteAsync(id);
        return await _unitOfWork.SaveChangesAsync();
    }

    public T Update(T entity)
    {
       return _repository.Update(entity);
    }
    public virtual async Task<int> UpdateAsync(T entity)
    {
        _repository.Update(entity);
        return await _unitOfWork.SaveChangesAsync();
    }

    public int GetCount(Expression<Func<T, bool>> expression)
    {
        return _repository.GetCount(expression);
    }

    public async Task<int> GetCountAsync(Expression<Func<T, bool>> expression = null)
    {
        return await _repository.GetCountAsync(expression);
    }

    public List<TResult> GetList<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return _repository.GetListByQuery(query);
    }

    public async Task<List<TResult>> GetListAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return await _repository.GetListByQueryAsync(query);
    }

    public TResult Get<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return _repository.GetByQuery(query);
    }

    public async Task<TResult> GetAsync<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        return await _repository.GetByQueryAsync(query);
    }
}