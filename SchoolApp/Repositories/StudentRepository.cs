using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;

namespace SchoolApp.Repositories;

public class StudentRepository : BaseRepository<Student>, IStudentRepository
{
    public StudentRepository(SchoolRest10Context context) : base(context)
    {
    }

    public async Task<Student?> GetByAm(string? am)
    {
        return await _context.Students
            .Where(s => s.Am == am)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();
    }

    public async Task<List<Course>> GetStudentCoursesAsync(int studentId)
    {
        List<Course> courses;

        courses = await _context.Students
            .Where(s => s.Id == studentId)
            .SelectMany(s => s.Courses)
            .ToListAsync();

        return courses;
    }

    public async Task<PaginatedResult<User>> GetPaginatedUsersStudentsAsync(int pageNumber, int pageSize,
        List<Expression<Func<User, bool>>> predicates)
    {
        IQueryable<User> query = _context.Users
            .Include(u => u.Student)                     // only for users that are students!
            .Where(u => u.Student != null);

        if (predicates is not null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);                // AND is implied here.
            }
        }

        int totalRecords = await query.CountAsync();          // is being executed

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = PaginationDefaults.DefaultPageSize;
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;

        int skip = (pageNumber - 1) * pageSize;      // number of pages we skip. pageNumber - 1 because in Db it starts.
                                                     // from 0. it works as offset.
        var data = await query
            .OrderBy(u => u.Id)                 // Paginated always with OrderBy BEFORE skip !!!!
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();                           // is being executed

        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }

    public override async Task<Student?> GetAsync(int id)
    {
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
