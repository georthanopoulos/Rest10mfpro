namespace SchoolApp.Repositories;

public interface IBaseRepository<T>          // ALL CRUDs in order to be used from all entities - called. 
{
    Task AddAsync(T entity);                  // we don't use generics (meaning T) when we don't want to return something. otherwise <T> is needed.

    Task AddRangeAsync(IEnumerable<T> entities);

    Task UpdateAsync(T entity);

    Task<bool> DeleteAsync(int id);

    Task<T?> GetAsync(int id);

    Task<IEnumerable<T>> GetAllAsync();

    Task<int> GetCountAsync();
}
