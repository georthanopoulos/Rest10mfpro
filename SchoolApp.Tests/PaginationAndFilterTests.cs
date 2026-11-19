using System.Net;

namespace SchoolApp.Tests;

public class PaginationAndFilterTests : ApiTestBase
{
    public PaginationAndFilterTests(ApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData("/api/v1/users")]
    [InlineData("/api/v1/teachers")]
    [InlineData("/api/v1/students")]
    [InlineData("/api/v1/courses")]
    public async Task Page_size_is_capped_at_100_and_bad_values_fall_back_to_defaults(string path)
    {
        var admin = await AdminAsync();

        var huge = await Read<Page<object>>(await admin.GetAsync($"{path}?pageSize=1000"));
        Assert.Equal(100, huge.PageSize);

        var zero = await Read<Page<object>>(await admin.GetAsync($"{path}?pageNumber=0&pageSize=0"));
        Assert.Equal(1, zero.PageNumber);
        Assert.Equal(10, zero.PageSize);
    }

    [Fact]
    public async Task Total_pages_follow_from_total_records_and_page_size()
    {
        var admin = await AdminAsync();
        await NewTeacherAsync(admin);
        await NewTeacherAsync(admin);
        await NewTeacherAsync(admin);

        var page = await Read<Page<TeacherDto>>(await admin.GetAsync("/api/v1/teachers?pageSize=2"));

        Assert.True(page.TotalRecords >= 3);
        Assert.Equal(2, page.Data.Count);
        Assert.Equal((int)Math.Ceiling(page.TotalRecords / 2.0), page.TotalPages);
    }

    [Fact]
    public async Task Users_can_be_filtered_by_role_and_an_unknown_role_returns_400()
    {
        var admin = await AdminAsync();
        await NewTeacherAsync(admin);
        await NewStudentAsync(admin);

        var teachers = await Read<Page<UserDto>>(await admin.GetAsync("/api/v1/users?userRole=TEACHER&pageSize=100"));
        Assert.NotEmpty(teachers.Data);
        Assert.All(teachers.Data, u => Assert.Equal("TEACHER", u.UserRole));

        var bogus = await admin.GetAsync("/api/v1/users?userRole=BOGUS");
        Assert.Equal(HttpStatusCode.BadRequest, bogus.StatusCode);
    }

    [Fact]
    public async Task Students_can_be_found_by_username_and_the_teacher_list_never_contains_students()
    {
        var admin = await AdminAsync();
        var student = await NewStudentAsync(admin);

        var found = await Read<Page<StudentDto>>(await admin.GetAsync($"/api/v1/students?username={student.Username}"));
        Assert.Single(found.Data);
        Assert.Equal(student.ProfileId, found.Data[0].Id);

        var inTeachers = await Read<Page<TeacherDto>>(await admin.GetAsync($"/api/v1/teachers?username={student.Username}"));
        Assert.Empty(inTeachers.Data);
    }
}
