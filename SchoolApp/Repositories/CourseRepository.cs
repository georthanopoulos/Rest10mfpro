using Microsoft.EntityFrameworkCore;
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
}
