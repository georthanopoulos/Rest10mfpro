using SchoolApp.Core;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories;

public interface IUserRepository : IBaseRepository<User>
{                                                       // Besides the Base CRUD actions, we defined in IBaseRepository we can state more actions here, by defining them below.
    Task<User?> GetUserByUsernameAsync(string username);

    Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize, List<Expression<Func<User, bool>>> predicates);
}
