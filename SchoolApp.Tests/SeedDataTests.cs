using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolApp.Data;
using SchoolApp.Security;

namespace SchoolApp.Tests;

public class SeedDataTests : ApiTestBase
{
    public SeedDataTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Every_Capabilities_constant_exists_in_the_seeded_data()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchoolRest10Context>();

        var seeded = await db.Capabilities.Select(c => c.Name).ToListAsync();

        var constants = typeof(Capabilities)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral)
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToList();

        Assert.Equal(21, constants.Count);
        Assert.Equal(constants.Count, seeded.Count);
        Assert.All(constants, name => Assert.Contains(name, seeded));
    }

    [Fact]
    public async Task Roles_have_the_expected_number_of_capabilities()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchoolRest10Context>();

        var counts = await db.Roles
            .Include(r => r.Capabilities)
            .ToDictionaryAsync(r => r.Name, r => r.Capabilities.Count);

        Assert.Equal(21, counts["ADMIN"]);
        Assert.Equal(11, counts["EMPLOYEE"]);
        Assert.Equal(1, counts["TEACHER"]);
        Assert.Equal(1, counts["STUDENT"]);
    }

    [Fact]
    public async Task The_seeder_creates_exactly_one_admin_that_must_change_the_password()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchoolRest10Context>();

        var admin = await db.Users.SingleAsync(u => u.Username == ApiFactory.AdminUsername);

        Assert.True(admin.MustChangePassword);
        Assert.NotEqual(ApiFactory.AdminTempPassword, admin.Password);     // stored as a BCrypt hash, never in plain text
        Assert.StartsWith("$2", admin.Password);
    }
}
