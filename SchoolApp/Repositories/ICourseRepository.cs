using SchoolApp.Core;
using SchoolApp.Models;

namespace SchoolApp.Repositories;

public interface ICourseRepository : IBaseRepository<Course>
{
    Task<List<Student>> GetCourseStudentsAsync(int courseId);

    Task<Teacher?> GetCourseTeacherAsync(int courseId);

    Task<PaginatedResult<Course>> GetPaginatedCoursesWithTeacherAsync(int pageNumber, int pageSize);

    Task<Course?> GetCourseWithStudentsAsync(int courseId);
}
