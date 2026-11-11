using SchoolApp.Core;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories;

public interface IStudentRepository : IBaseRepository<Student>
{
    Task<List<Course>> GetStudentCoursesAsync(int studentId);

    Task<Student?> GetByAm(string? am);

    Task<PaginatedResult<User>> GetPaginatedUsersStudentsAsync(int pageNumber, int pageSize,
        List<Expression<Func<User, bool>>> predicates);
}
