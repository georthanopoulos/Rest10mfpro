using System.Net;
using System.Net.Http.Json;

namespace SchoolApp.Tests;

public class CourseTests : ApiTestBase
{
    public CourseTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Admin_can_create_and_read_a_course()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);

        var created = await NewCourseAsync(admin, teacher.ProfileId);
        Assert.Equal(teacher.ProfileId, created.TeacherId);
        Assert.Equal("Teacher", created.TeacherLastname);

        var get = await admin.GetAsync($"/api/v1/courses/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var missing = await admin.GetAsync("/api/v1/courses/999999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Course_with_an_unknown_teacher_or_a_bad_description_returns_400()
    {
        var admin = await AdminAsync();

        var unknownTeacher = await admin.PostAsJsonAsync("/api/v1/courses",
            new { description = "Databases", teacherId = 999999 });
        Assert.Equal(HttpStatusCode.BadRequest, unknownTeacher.StatusCode);

        var shortDescription = await admin.PostAsJsonAsync("/api/v1/courses",
            new { description = "x", teacherId = (int?)null });
        Assert.Equal(HttpStatusCode.BadRequest, shortDescription.StatusCode);
    }

    [Fact]
    public async Task Enrollment_lifecycle_returns_the_documented_status_codes()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);
        var student = await NewStudentAsync(admin);
        var course = await NewCourseAsync(admin, teacher.ProfileId);

        var enrollUrl = $"/api/v1/courses/{course.Id}/students/{student.ProfileId}";

        Assert.Equal(HttpStatusCode.NoContent, (await admin.PostAsync(enrollUrl, null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsync(enrollUrl, null)).StatusCode);

        Assert.Equal(HttpStatusCode.NotFound,
            (await admin.PostAsync($"/api/v1/courses/999999/students/{student.ProfileId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await admin.PostAsync($"/api/v1/courses/{course.Id}/students/999999", null)).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync(enrollUrl)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await admin.DeleteAsync(enrollUrl)).StatusCode);
    }

    [Fact]
    public async Task Employee_can_enroll_but_a_student_cannot_enroll_themselves()
    {
        var admin = await AdminAsync();
        var employee = await EmployeeAsync();
        var student = await NewStudentAsync(admin);
        var course = await NewCourseAsync(admin);

        var url = $"/api/v1/courses/{course.Id}/students/{student.ProfileId}";

        Assert.Equal(HttpStatusCode.Forbidden, (await student.Client.PostAsync(url, null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await employee.PostAsync(url, null)).StatusCode);
    }

    [Fact]
    public async Task Teacher_and_student_see_their_own_courses()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);
        var student = await NewStudentAsync(admin);
        var course = await NewCourseAsync(admin, teacher.ProfileId);

        await admin.PostAsync($"/api/v1/courses/{course.Id}/students/{student.ProfileId}", null);

        var teacherCourses = await Read<Page<CourseDto>>(
            await teacher.Client.GetAsync($"/api/v1/teachers/{teacher.ProfileId}/courses"));
        Assert.Contains(teacherCourses.Data, c => c.Id == course.Id);

        var studentCourses = await Read<Page<CourseDto>>(
            await student.Client.GetAsync($"/api/v1/students/{student.ProfileId}/courses"));
        Assert.Contains(studentCourses.Data, c => c.Id == course.Id);

        // after unenrolling the student no longer sees it
        await admin.DeleteAsync($"/api/v1/courses/{course.Id}/students/{student.ProfileId}");
        var after = await Read<Page<CourseDto>>(
            await student.Client.GetAsync($"/api/v1/students/{student.ProfileId}/courses"));
        Assert.DoesNotContain(after.Data, c => c.Id == course.Id);
    }

    [Fact]
    public async Task A_teacher_with_courses_cannot_be_deleted_until_the_courses_are_gone()
    {
        var admin = await AdminAsync();
        var teacher = await NewTeacherAsync(admin);
        var course = await NewCourseAsync(admin, teacher.ProfileId);

        var blocked = await admin.DeleteAsync($"/api/v1/teachers/{teacher.ProfileId}");
        Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/v1/courses/{course.Id}")).StatusCode);

        var allowed = await admin.DeleteAsync($"/api/v1/teachers/{teacher.ProfileId}");
        Assert.Equal(HttpStatusCode.NoContent, allowed.StatusCode);

        var gone = await admin.GetAsync($"/api/v1/teachers/{teacher.ProfileId}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }
}
