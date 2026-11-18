namespace SchoolApp.Security;

public static class Capabilities
{
    public const string InsertTeacher = "INSERT_TEACHER";
    public const string ViewTeachers = "VIEW_TEACHERS";
    public const string ViewTeacher = "VIEW_TEACHER";
    public const string EditTeacher = "EDIT_TEACHER";
    public const string DeleteTeacher = "DELETE_TEACHER";
    public const string ViewOnlyTeacher = "VIEW_ONLY_TEACHER";

    public const string InsertStudent = "INSERT_STUDENT";
    public const string ViewStudents = "VIEW_STUDENTS";
    public const string ViewStudent = "VIEW_STUDENT";
    public const string EditStudent = "EDIT_STUDENT";
    public const string DeleteStudent = "DELETE_STUDENT";
    public const string ViewOnlyStudent = "VIEW_ONLY_STUDENT";

    public const string InsertCourse = "INSERT_COURSE";
    public const string ViewCourses = "VIEW_COURSES";
    public const string ViewCourse = "VIEW_COURSE";
    public const string EditCourse = "EDIT_COURSE";
    public const string DeleteCourse = "DELETE_COURSE";

    public const string ViewUsers = "VIEW_USERS";
    public const string InsertAdmin = "INSERT_ADMIN";
    public const string EnrollStudent = "ENROLL_STUDENT";
    public const string UnenrollStudent = "UNENROLL_STUDENT";
}