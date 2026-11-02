using SchoolApp.Data;

namespace SchoolApp.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SchoolRest10Context _context;              // Same context through ALL repositories! IMPORTANT!

    public IUserRepository UserRepository { get; }

    public ITeacherRepository TeacherRepository { get; }

    public IStudentRepository StudentRepository { get; }

    public ICourseRepository CourseRepository { get; }

    public IRoleRepository RoleRepository { get; }

    public IActivationTokenRepository ActivationTokenRepository { get; }

    public UnitOfWork(SchoolRest10Context context)
    {
        _context = context;
        UserRepository = new UserRepository(context);
        StudentRepository = new StudentRepository(context);
        TeacherRepository = new TeacherRepository(context);
        CourseRepository = new CourseRepository(context);
        RoleRepository = new RoleRepository(context);
        ActivationTokenRepository = new ActivationTokenRepository(context);
    }

    public async Task<bool> SaveAsync()
    {
        return await _context.SaveChangesAsync() > 0;     // performs the commit and then automatically rolls back in case of failure.
    }

}
