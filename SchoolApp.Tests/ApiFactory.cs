using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolApp.Data;

namespace SchoolApp.Tests;

/// <summary>
/// Starts the real application in memory. SQL Server is replaced by an in-memory SQLite database,
/// so the tests need neither Docker nor a running SQL Server. Every factory (= every test class)
/// gets its own empty database that is created from the EF model, including the seed data.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminTempPassword = "Temp-Passw0rd!";

    private readonly SqliteConnection _connection;

    static ApiFactory()
    {
        // Program.cs reads these settings while it builds the host,
        // so environment variables are the most reliable way to supply them.
        Environment.SetEnvironmentVariable("Jwt__Issuer", "https://tests.local");
        Environment.SetEnvironmentVariable("Jwt__Audience", "https://tests.local");
        Environment.SetEnvironmentVariable("Jwt__Secret", "test-only-secret-that-is-at-least-32-bytes-long-0123456789");
        Environment.SetEnvironmentVariable("Cors__Origin", "http://localhost:3000");
        Environment.SetEnvironmentVariable("Seed__AdminUsername", AdminUsername);
        Environment.SetEnvironmentVariable("Seed__AdminEmail", "admin@tests.local");
        Environment.SetEnvironmentVariable("Seed__AdminPassword", AdminTempPassword);
        Environment.SetEnvironmentVariable("ConnectionStrings__DevConnection", "Server=unused;Database=unused");
    }

    public ApiFactory()
    {
        // An in-memory SQLite database lives only while its connection stays open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SchoolRest10Context>()
            .UseSqlite(_connection)
            .Options;

        using var context = new SchoolRest10Context(options);
        context.Database.EnsureCreated();     // must exist before the app starts, because the DbSeeder runs at startup
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");    // no Swagger, no Development-only behaviour

        builder.ConfigureTestServices(services =>
        {
            // Remove everything that Program.cs registered for the SQL Server DbContext ...
            var toRemove = services
                .Where(d => d.ServiceType == typeof(SchoolRest10Context)
                         || (d.ServiceType.IsGenericType
                             && d.ServiceType.GenericTypeArguments.Contains(typeof(SchoolRest10Context))))
                .ToList();

            foreach (var descriptor in toRemove)
            {
                services.Remove(descriptor);
            }

            // ... and register the same context on SQLite.
            services.AddDbContext<SchoolRest10Context>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
