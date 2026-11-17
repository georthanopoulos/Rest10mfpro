using SchoolApp.Core;
using SchoolApp.DTO;

namespace SchoolApp.Services;

public interface ICourseService
{
    Task<CourseReadOnlyDTO> InsertCourseAsync(CourseInsertDTO request);

    Task<PaginatedResult<CourseReadOnlyDTO>> GetPaginatedCoursesAsync(int pageNumber, int pageSize);

    Task<CourseReadOnlyDTO> GetCourseByIdAsync(int id);

    Task<CourseReadOnlyDTO> UpdateCourseAsync(int id, CourseUpdateDTO request);

    Task DeleteCourseAsync(int id);

    Task EnrollStudentAsync(int courseId, int studentId);

    Task UnenrollStudentAsync(int courseId, int studentId);
}