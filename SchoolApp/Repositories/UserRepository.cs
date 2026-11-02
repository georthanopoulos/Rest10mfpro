using Microsoft.EntityFrameworkCore;
using SchoolApp.Core;
using SchoolApp.Data;
using SchoolApp.Models;
using System.Linq.Expressions;


namespace SchoolApp.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository       // first right click on IUserRepository and implement. And Then right click on UserRepository and generate.
{                                                                            // Firstly we entend and then implement the Interface. NOT Vice versa!!!
    public UserRepository(SchoolRest10Context context) : base(context)
    {
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)          // Eager Loading - Join
                .ThenInclude(r => r.Capabilities)
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);
    }

    public async Task<PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize,
        List<Expression<Func<User, bool>>> predicates)
    {
        IQueryable<User> query = _context.Users                // is NOT being executed yet.
            .Include(u => u.Role);

        if (predicates is not null && predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);              // AND is implied here.
            }
        }

        int totalRecords = await query.CountAsync();                                                       // is being executed

        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        int skip = (pageNumber - 1) * pageSize;      // number of pages we skip. pageNumber - 1 beacause in Db it starts.
                                                     // from 0. it works as offset.
        var data = await query
            .OrderBy(u => u.Id)                 // Paginated always with OrderBy BEFORE skip !!!!
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();                                                                          // is being executed

        return new PaginatedResult<User>(data, totalRecords, pageNumber, pageSize);
    }

    public override async Task<User?> GetAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameOrEmailExistsAsync(string username, string email)
    {
        return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
    }
}
