namespace SchoolApp.Services;

public class ApplicationService : IApplicationService
{

    public IUserService UserService { get; }

    public ITeacherService TeacherService { get; }

    public IStudentService StudentService { get; }

    public ICourseService CourseService { get; }

    // Other services can be added

    public ApplicationService(IUserService userService, ITeacherService teacherService,
        IStudentService studentService, ICourseService courseService)
    {
        UserService = userService;
        TeacherService = teacherService;
        StudentService = studentService;
        CourseService = courseService;
    }
}