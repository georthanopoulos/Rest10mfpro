using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;
using SchoolApp.Security;

namespace SchoolApp.Data;

public class DbSeeder
{
    private readonly SchoolRest10Context _context;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionUtil _encryptionUtil;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(SchoolRest10Context context, IConfiguration configuration,
        IEncryptionUtil encryptionUtil, ILogger<DbSeeder> logger)
    {
        _context = context;
        _configuration = configuration;
        _encryptionUtil = encryptionUtil;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        string? adminUsername = _configuration["Seed:AdminUsername"];
        string? adminEmail = _configuration["Seed:AdminEmail"];
        string? adminPassword = _configuration["Seed:AdminPassword"];

        bool adminExists = await _context.Users.AnyAsync(u => u.Role.Name == "ADMIN");

        if (adminExists)
        {
            _logger.LogInformation("Admin already exists, skipping admin seed.");
            return;
        }

        if (string.IsNullOrWhiteSpace(adminUsername)
            || string.IsNullOrWhiteSpace(adminEmail)
            || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("Missing admin seed settings:" +
                " Seed:AdminUsername, Seed:AdminEmail, Seed:AdminPassword");
        }

        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "ADMIN");

        var adminUser = new User
        {
            Username = adminUsername,
            Password = _encryptionUtil.Encrypt(adminPassword),
            Email = adminEmail,
            Firstname = "System",
            Lastname = "Administrator",
            Role = adminRole,
            Status = AccountStatus.Active,
            MustChangePassword = true
        };

        _context.Users.Add(adminUser);

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Admin user with username={Username} created successfully.", adminUsername);
    }
}
