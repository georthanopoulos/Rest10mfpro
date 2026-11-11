using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories;

public class TeacherRepository : BaseRepository<Teacher>, ITeacherRepository
{
    public TeacherRepository(SchoolRest10Context context) : base(context)
    {
    }

    public async Task<Teacher?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Teachers
            .Where(t => t.PhoneNumber == phoneNumber)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();
    }

    public async Task<PaginatedResult<User>> GetPaginatedUsersTeachersAsync(int pageNumber, int pageSize,
        List<Expression<Func<User, bool>>> predicates)
    {
        IQueryable<User> query = _context.Users   
            .Include(u => u.Teacher)                     // only for users that are teachers!
            .Where(u => u.Teacher != null);

        if (predicates is not null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);                // AND is implied here.
            }
        }

        int totalRecords = await query.CountAsync();          // is being executed

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        
        int skip = (pageNumber - 1) * pageSize;      // number of pages we skip. pageNumber - 1 because in Db it starts.
                                                     // from 0. it works as offset.
        var data = await query
            .OrderBy(u => u.Id)                 // Paginated always with OrderBy BEFORE skip !!!!
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();                             // is being executed

        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }

    public async Task<List<Course>> GetTeacherCoursesAsync(int teacherId)
    {
        List<Course> courses;

        courses = await _context.Teachers
            .Where(t => t.Id == teacherId)
            .SelectMany(t => t.Courses)
            .ToListAsync();

        return courses;
    }

    public override async Task<Teacher?> GetAsync(int id)
    {
        return await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
