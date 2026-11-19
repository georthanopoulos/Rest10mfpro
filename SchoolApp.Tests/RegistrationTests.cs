using System.Net;

namespace SchoolApp.Tests;

public class RegistrationTests : ApiTestBase
{
    public RegistrationTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Admin_can_pre_register_a_teacher()
    {
        var admin = await AdminAsync();

        var response = await PostTeacherAsync(admin, Unique("teacher"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var result = await Read<PreRegResult>(response);
        Assert.Equal("TEACHER", result.User.UserRole);
        Assert.Equal(64, result.ActivationToken.Length);
        Assert.True(result.ExpiresAt > DateTime.UtcNow.AddHours(71));
    }

    [Fact]
    public async Task Employee_can_pre_register_a_student()
    {
        var employee = await EmployeeAsync();

        var response = await PostStudentAsync(employee, Unique("student"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("STUDENT", (await Read<PreRegResult>(response)).User.UserRole);
    }

    [Fact]
    public async Task Anonymous_gets_401_and_a_teacher_gets_403()
    {
        var anonymous = await PostTeacherAsync(Anonymous(), Unique("teacher"));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var teacher = await NewTeacherAsync();
        var forbidden = await PostTeacherAsync(teacher.Client, Unique("teacher"));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task Duplicate_username_email_or_phone_returns_409()
    {
        var admin = await AdminAsync();
        var username = Unique("teacher");
        var phone = UniquePhone();

        Assert.Equal(HttpStatusCode.Created, (await PostTeacherAsync(admin, username, phone)).StatusCode);

        var sameUsername = await PostTeacherAsync(admin, username);
        Assert.Equal(HttpStatusCode.Conflict, sameUsername.StatusCode);

        var sameEmail = await PostTeacherAsync(admin, Unique("teacher"), email: $"{username}@example.com");
        Assert.Equal(HttpStatusCode.Conflict, sameEmail.StatusCode);

        var samePhone = await PostTeacherAsync(admin, Unique("teacher"), phone);
        Assert.Equal(HttpStatusCode.Conflict, samePhone.StatusCode);
    }

    [Fact]
    public async Task Duplicate_student_am_returns_409()
    {
        var admin = await AdminAsync();
        var am = Unique("am");

        Assert.Equal(HttpStatusCode.Created, (await PostStudentAsync(admin, Unique("student"), am)).StatusCode);

        var duplicate = await PostStudentAsync(admin, Unique("student"), am);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Invalid_body_returns_400()
    {
        var admin = await AdminAsync();

        var response = await admin.PostAsync("/api/v1/teachers",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_deleted_teacher_keeps_the_username_reserved()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);

        var deleted = await admin.DeleteAsync($"/api/v1/teachers/{teacher.ProfileId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var again = await PostTeacherAsync(admin, teacher.Username);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task A_deleted_student_disappears_and_cannot_log_in()
    {
        var admin = await AdminAsync();
        var student = await NewStudentAsync(admin);

        var deleted = await admin.DeleteAsync($"/api/v1/students/{student.ProfileId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var get = await admin.GetAsync($"/api/v1/students/{student.ProfileId}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        var login = await LoginRawAsync(student.Username, Password);
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }
}
