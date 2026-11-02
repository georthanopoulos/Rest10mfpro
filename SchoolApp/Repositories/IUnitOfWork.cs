namespace SchoolApp.Repositories;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }                   // property → PascalCase, public property

    IStudentRepository StudentRepository { get; }

    ITeacherRepository TeacherRepository { get; }

    ICourseRepository CourseRepository { get; }

    IRoleRepository RoleRepository { get; }

    IActivationTokenRepository ActivationTokenRepository { get; }

    Task<bool> SaveAsync();

}
