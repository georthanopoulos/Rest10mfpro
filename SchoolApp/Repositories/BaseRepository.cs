using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;

namespace SchoolApp.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class     // skeletal implementation. Also the expression "where T : class" is reference type, definition.
    {                                                                          // right click on IBaseRepository and impleent interface -> produces all methods below.
        protected readonly SchoolRest10Context _context;         // DbContext -> Secretary
        protected readonly DbSet<T> _dbSet;                        // DbSet -> dossier


        public BaseRepository(SchoolRest10Context context)          // constructor injection
        {
            _context = context;
            _dbSet = context.Set<T>();                 // populate dynamically retrieves DbSet. through context.Set, if T = student we get DbSet of student. If T=Teacher etc.
        }


        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);      // characterized as async in order to do await within its body and virtual in order to become override from the below ones.

        public virtual async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public virtual Task UpdateAsync(T entity)                // The update doesn't need async. Does not have await. However, we have something close to that, the "CompletedTask" at the end.
        {                                                             // In reality this method will NOT be actually used. It is not the right process, regarding to the communication with the db.
            _dbSet.Attach(entity);                                         // We will perform the Update process through Service layer instead.
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;                                // NEVER USE THIS TYPE OF UPDATE! The update will be done through service layer. This is a bad Update method.
        }

        public virtual async Task<bool> DeleteAsync(int id)     // It is an async delete because we retrieve it first, before removal.
        {
            T? existingEntity = await _dbSet.FindAsync(id);
            if (existingEntity is null) return false;
            _dbSet.Remove(existingEntity);
            return true;
        }

        public virtual async Task<T?> GetAsync(int id) => await _dbSet.FindAsync(id);

        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<int> GetCountAsync() => await _dbSet.CountAsync();
    }
}
