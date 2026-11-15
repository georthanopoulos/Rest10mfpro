namespace SchoolApp.Services;

public interface IApplicationService
{
    IUserService UserService { get; }

    ITeacherService TeacherService { get; }

    IStudentService StudentService { get; }

    ICourseService CourseService { get; }

    // OtherService can be added { get; }
}
