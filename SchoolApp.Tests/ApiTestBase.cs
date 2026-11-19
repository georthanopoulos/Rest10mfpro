using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolApp.Data;
using SchoolApp.Models;
using SchoolApp.Security;

namespace SchoolApp.Tests;

public abstract class ApiTestBase : IClassFixture<ApiFactory>
{
    protected const string Password = "Passw0rd!x1";

    private static int _counter;

    protected ApiTestBase(ApiFactory factory)
    {
        Factory = factory;
    }

    protected ApiFactory Factory { get; }

    // ---------- response shapes (only the fields the tests need) ----------

    protected record LoginResult(string Token, bool MustChangePassword, string? Message);
    protected record UserDto(int Id, string Username, string UserRole);
    protected record PreRegResult(UserDto User, string ActivationToken, DateTime ExpiresAt);
    protected record TeacherDto(int Id, int UserId, string Username);
    protected record StudentDto(int Id, int UserId, string Username, string Am);
    protected record CourseDto(int Id, string Description, int? TeacherId, string? TeacherLastname);
    protected record Page<T>(List<T> Data, int TotalRecords, int PageNumber, int PageSize, int TotalPages);

    /// <summary>A fully activated teacher or student: ids plus a client that is already logged in.</summary>
    protected record Person(int UserId, int ProfileId, string Username, HttpClient Client);

    // ---------- small helpers ----------

    protected static string Unique(string prefix) => $"{prefix}{Interlocked.Increment(ref _counter)}";

    protected static string UniquePhone() => "69" + Interlocked.Increment(ref _counter).ToString("D8");

    protected static async Task<T> Read<T>(HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<T>())!;

    protected static async Task<string> DetailAsync(HttpResponseMessage response)
    {
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("detail").GetString() ?? "";
    }

    protected HttpClient Anonymous() => Factory.CreateClient();

    protected static HttpClient WithBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ---------- authentication ----------

    protected async Task<HttpResponseMessage> LoginRawAsync(string username, string password, HttpClient? client = null)
        => await (client ?? Anonymous()).PostAsJsonAsync("/api/v1/auth/login", new { username, password });

    protected async Task<HttpClient> ClientForAsync(string username, string password = Password)
    {
        var response = await LoginRawAsync(username, password);
        response.EnsureSuccessStatusCode();

        var result = await Read<LoginResult>(response);
        return WithBearer(Anonymous(), result.Token);
    }

    protected async Task ActivateAsync(string token, string password = Password)
    {
        var response = await Anonymous().PostAsJsonAsync("/api/v1/auth/activate", new { token, password });
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Creates an active user with the given role straight in the database
    /// (there is no endpoint for creating employees or admins). Returns the username.
    /// </summary>
    protected async Task<string> CreateUserAsync(string roleName, bool mustChangePassword = false)
    {
        var username = Unique(roleName.ToLowerInvariant());

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchoolRest10Context>();
        var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionUtil>();

        var role = await db.Roles.FirstAsync(r => r.Name == roleName);

        db.Users.Add(new User
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = encryption.Encrypt(Password),
            Firstname = "Test",
            Lastname = roleName,
            Role = role,
            Status = AccountStatus.Active,
            MustChangePassword = mustChangePassword
        });

        await db.SaveChangesAsync();
        return username;
    }

    protected async Task<HttpClient> AdminAsync() => await ClientForAsync(await CreateUserAsync("ADMIN"));

    protected async Task<HttpClient> EmployeeAsync() => await ClientForAsync(await CreateUserAsync("EMPLOYEE"));

    // ---------- pre-registration ----------

    protected async Task<HttpResponseMessage> PostTeacherAsync(HttpClient registrar, string username, string? phone = null, string? email = null)
        => await registrar.PostAsJsonAsync("/api/v1/teachers", new
        {
            username,
            email = email ?? $"{username}@example.com",
            firstname = "Test",
            lastname = "Teacher",
            phoneNumber = phone ?? UniquePhone(),
            institution = "AUEB"
        });

    protected async Task<HttpResponseMessage> PostStudentAsync(HttpClient registrar, string username, string? am = null, string? email = null)
        => await registrar.PostAsJsonAsync("/api/v1/students", new
        {
            username,
            email = email ?? $"{username}@example.com",
            firstname = "Test",
            lastname = "Student",
            am = am ?? Unique("am"),
            institution = "AUEB",
            department = "Informatics"
        });

    protected async Task<PreRegResult> PreRegisterTeacherAsync(HttpClient registrar, string? username = null)
    {
        var response = await PostTeacherAsync(registrar, username ?? Unique("teacher"));
        response.EnsureSuccessStatusCode();
        return await Read<PreRegResult>(response);
    }

    protected async Task<PreRegResult> PreRegisterStudentAsync(HttpClient registrar, string? username = null)
    {
        var response = await PostStudentAsync(registrar, username ?? Unique("student"));
        response.EnsureSuccessStatusCode();
        return await Read<PreRegResult>(response);
    }

    /// <summary>Pre-registers, activates and logs in a teacher.</summary>
    protected async Task<Person> NewTeacherAsync(HttpClient? registrar = null)
    {
        registrar ??= await AdminAsync();

        var pre = await PreRegisterTeacherAsync(registrar);
        await ActivateAsync(pre.ActivationToken);

        var client = await ClientForAsync(pre.User.Username);
        var page = await Read<Page<TeacherDto>>(
            await registrar.GetAsync($"/api/v1/teachers?username={pre.User.Username}"));

        return new Person(pre.User.Id, page.Data.Single().Id, pre.User.Username, client);
    }

    /// <summary>Pre-registers, activates and logs in a student.</summary>
    protected async Task<Person> NewStudentAsync(HttpClient? registrar = null)
    {
        registrar ??= await AdminAsync();

        var pre = await PreRegisterStudentAsync(registrar);
        await ActivateAsync(pre.ActivationToken);

        var client = await ClientForAsync(pre.User.Username);
        var page = await Read<Page<StudentDto>>(
            await registrar.GetAsync($"/api/v1/students?username={pre.User.Username}"));

        return new Person(pre.User.Id, page.Data.Single().Id, pre.User.Username, client);
    }

    // ---------- courses ----------

    protected async Task<CourseDto> NewCourseAsync(HttpClient admin, int? teacherId = null)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/courses", new { description = "Databases", teacherId });
        response.EnsureSuccessStatusCode();
        return await Read<CourseDto>(response);
    }
}
