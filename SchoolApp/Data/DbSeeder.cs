using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public class DbSeeder
{
    private readonly SchoolRest10Context _context;
    private readonly IConfiguration _configuration;

    public DbSeeder(SchoolRest10Context context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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

        string? adminUsername = _configuration["Seed:AdminUsername"];
        string? adminEmail = _configuration["Seed:AdminEmail"];
        string? adminPassword = _configuration["Seed:AdminPassword"];
    }
}
