using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public class DbSeeder
{
    private readonly SchoolRest10Context _context;

    public DbSeeder(SchoolRest10Context context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        List<string> roleNames = new() { "ADMIN", "EMPLOYEE", "TEACHER", "STUDENT" };

        
        foreach(var roleName in roleNames)
        {    
            bool exists = await _context.Roles.AnyAsync(r => r.Name == roleName);

            if (!exists)
            {
                _context.Roles.Add(new Role { Name = roleName });
            }
        }

        await _context.SaveChangesAsync();
    }
}
