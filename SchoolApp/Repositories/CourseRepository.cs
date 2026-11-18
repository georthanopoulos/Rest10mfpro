using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories;

public class CourseRepository : BaseRepository<Course>, ICourseRepository
{
    public CourseRepository(SchoolRest10Context context) : base(context)
    {
    }

    public async Task<List<Student>> GetCourseStudentsAsync(int courseId)
    {
        return await _context.Courses
            .Where(c => c.Id == courseId)            // Filtering
            .SelectMany(c => c.Students)                // Mapping
            .ToListAsync();
    }

    public async Task<Teacher?> GetCourseTeacherAsync(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Teacher)                      // Join first - it is better this way
            .SingleOrDefaultAsync(c => c.Id == courseId);     // this works as "WHERE". It is the same. SingleOrDefault  -> it means 0 or 1. Used instead of FirstOrDefault. It is more readable and comprehensive regarding the 0 / 1 result.

        return course?.Teacher;
    }

    public async Task<Course?> GetCourseWithStudentsAsync(int courseId)
    {
        return await _context.Courses
            .Include(c => c.Students)
            .SingleOrDefaultAsync(c => c.Id == courseId);
    }

    public override async Task<Course?> GetAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PaginatedResult<Course>> GetPaginatedCoursesWithTeacherAsync(int pageNumber, int pageSize)
    {
        IQueryable<Course> query = _context.Courses
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User);

        return await query
            .OrderBy(c => c.Id)
            .ToPaginatedResultAsync(pageNumber, pageSize);
    }

    public async Task<PaginatedResult<Course>> GetPaginatedCoursesByTeacherAsync(int teacherId, int pageNumber, int pageSize)
    {
        IQueryable<Course> query = _context.Courses
            .Where(c => c.TeacherId == teacherId)
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User);

        return await query
            .OrderBy(c => c.Id)
            .ToPaginatedResultAsync(pageNumber, pageSize);
    }

    public async Task<PaginatedResult<Course>> GetPaginatedCoursesByStudentAsync(int studentId, int pageNumber, int pageSize)
    {
        IQueryable<Course> query = _context.Courses
            .Where(c => c.Students.Any(s => s.Id == studentId))
            .Include(c => c.Teacher)
                .ThenInclude(t => t.User);

        return await query
            .OrderBy(c => c.Id)
            .ToPaginatedResultAsync(pageNumber, pageSize);
    }
}
