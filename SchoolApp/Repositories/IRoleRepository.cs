using SchoolApp.Models;

namespace SchoolApp.Repositories;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
}