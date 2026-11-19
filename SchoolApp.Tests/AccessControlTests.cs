using System.Net;
using System.Net.Http.Json;

namespace SchoolApp.Tests;

public class AccessControlTests : ApiTestBase
{
    public AccessControlTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Protected_endpoints_return_401_without_a_token()
    {
        var client = Anonymous();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/teachers")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/students")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/courses")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users/1")).StatusCode);
    }

    [Fact]
    public async Task A_user_can_read_their_own_profile_but_not_someone_elses()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);
        var student = await NewStudentAsync(admin);

        Assert.Equal(HttpStatusCode.OK, (await teacher.Client.GetAsync($"/api/v1/users/{teacher.UserId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync($"/api/v1/users/{student.UserId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.Client.GetAsync($"/api/v1/users/{teacher.UserId}")).StatusCode);
    }

    [Fact]
    public async Task Only_roles_with_VIEW_USERS_can_list_users()
    {
        var admin = await AdminAsync();
        var employee = await EmployeeAsync();
        var teacher = await NewTeacherAsync(admin);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/v1/users")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await employee.GetAsync("/api/v1/users")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync("/api/v1/users")).StatusCode);
    }

    [Fact]
    public async Task Teachers_cannot_list_teachers_or_students()
    {
        var teacher = await NewTeacherAsync();

        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync("/api/v1/teachers")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync("/api/v1/students")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync("/api/v1/courses")).StatusCode);
    }

    [Fact]
    public async Task Teacher_profile_is_visible_to_the_owner_and_to_employees_only()
    {
        var admin = await AdminAsync();
        var employee = await EmployeeAsync();
        var teacher = await NewTeacherAsync(admin);
        var otherTeacher = await NewTeacherAsync(admin);
        var student = await NewStudentAsync(admin);

        var url = $"/api/v1/teachers/{teacher.ProfileId}";

        Assert.Equal(HttpStatusCode.OK, (await teacher.Client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await employee.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherTeacher.Client.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await student.Client.GetAsync(url)).StatusCode);
    }

    [Fact]
    public async Task Student_profile_and_courses_are_visible_to_the_owner_only()
    {
        var admin = await AdminAsync();
        var student = await NewStudentAsync(admin);
        var otherStudent = await NewStudentAsync(admin);
        var teacher = await NewTeacherAsync(admin);

        var profile = $"/api/v1/students/{student.ProfileId}";
        var courses = $"/api/v1/students/{student.ProfileId}/courses";

        Assert.Equal(HttpStatusCode.OK, (await student.Client.GetAsync(profile)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await student.Client.GetAsync(courses)).StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden, (await otherStudent.Client.GetAsync(profile)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherStudent.Client.GetAsync(courses)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await teacher.Client.GetAsync(courses)).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync(courses)).StatusCode);
    }

    [Fact]
    public async Task By_username_does_not_reveal_which_usernames_exist()
    {
        var admin = await AdminAsync();
        var student = await NewStudentAsync(admin);
        var teacher = await NewTeacherAsync(admin);

        // a student asking about a name that does not exist gets the same 403 as for a name that does exist
        var missing = await student.Client.GetAsync("/api/v1/users/by-username/nobody-here");
        var existing = await student.Client.GetAsync($"/api/v1/users/by-username/{teacher.Username}");
        Assert.Equal(HttpStatusCode.Forbidden, missing.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, existing.StatusCode);

        // the owner can read their own record
        var own = await student.Client.GetAsync($"/api/v1/users/by-username/{student.Username}");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);

        // somebody who may view every user gets the honest 404
        var adminMissing = await admin.GetAsync("/api/v1/users/by-username/nobody-here");
        Assert.Equal(HttpStatusCode.NotFound, adminMissing.StatusCode);
    }

    [Fact]
    public async Task Employee_cannot_delete_or_create_courses_but_admin_can()
    {
        var admin = await AdminAsync();
        var employee = await EmployeeAsync();
        var teacher = await NewTeacherAsync(admin);

        var forbiddenCreate = await employee.PostAsJsonAsync("/api/v1/courses", new { description = "Databases", teacherId = teacher.ProfileId });
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenCreate.StatusCode);

        var forbiddenDelete = await employee.DeleteAsync($"/api/v1/teachers/{teacher.ProfileId}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenDelete.StatusCode);

        var course = await NewCourseAsync(admin, teacher.ProfileId);
        Assert.Equal("Databases", course.Description);
    }
}
