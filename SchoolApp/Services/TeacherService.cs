using AutoMapper;
using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;
using System.Linq.Expressions;

namespace SchoolApp.Services;

public class TeacherService : ITeacherService
{
    private readonly IActivationTokenUtil _activationTokenUtil;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(IActivationTokenUtil activationTokenUtil,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<TeacherService> logger)
    {
        _activationTokenUtil = activationTokenUtil;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PreRegistrationResultDTO> PreRegisterAsync(TeacherPreRegisterDTO request)
    {
        bool exists = await _unitOfWork.UserRepository.UsernameOrEmailExistsAsync(request.Username!, request.Email!);

        if (exists)
        {
            throw new EntityAlreadyExistsException("User", "A user with this username or email already exists.");
        }

        Teacher? existingTeacher = await _unitOfWork.TeacherRepository.GetByPhoneNumberAsync(request.PhoneNumber!);

        if (existingTeacher is not null)
        {
            throw new EntityAlreadyExistsException("Teacher", "A teacher with this phone number already exists.");
        }

        Role? teacherRole = await _unitOfWork.RoleRepository.GetByNameAsync("TEACHER");

        if (teacherRole is null)
        {
            throw new InvalidOperationException("Role TEACHER is missing from the database.");
        }

        User user = _mapper.Map<User>(request);
        Teacher teacher = _mapper.Map<Teacher>(request);

        user.Role = teacherRole;
        user.Status = AccountStatus.Pending;
        user.Password = null;
        user.Teacher = teacher;

        string rawToken = _activationTokenUtil.GenerateToken();

        ActivationToken activationToken = new ActivationToken
        {
            User = user,
            TokenHash = _activationTokenUtil.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(72)
        };

        await _unitOfWork.UserRepository.AddAsync(user);           // teacher is strictly binded to the User, therefore AddAsync is not needed for teacher. It will be synchronized automatically.
        await _unitOfWork.ActivationTokenRepository.AddAsync(activationToken);

        await _unitOfWork.SaveAsync();     // teacher, user, token arfe all saved within the same transaction (due to _unitOfWork)

        _logger.LogInformation("User with id={Id} and username={Username} pre-registered successfully", user.Id, user.Username);     // NEVER include the raw token in the logs.

        return new PreRegistrationResultDTO
        {
            User = _mapper.Map<UserReadOnlyDTO>(user),
            ActivationToken = rawToken,
            ExpiresAt = activationToken.ExpiresAt
        };
    }

    public async Task<PaginatedResult<TeacherReadOnlyDTO>> GetPaginatedTeachersFilteredAsync(int pageNumber,
        int pageSize, TeacherFiltersDTO filters)
    {
        List<Expression<Func<User, bool>>> predicates = [];

        if (!string.IsNullOrEmpty(filters.Username))
        {
            predicates.Add(u => u.Username == filters.Username);
        }

        if (!string.IsNullOrEmpty(filters.Email))
        {
            predicates.Add(u => u.Email == filters.Email);
        }

        if (!string.IsNullOrEmpty(filters.Lastname))
        {
            predicates.Add(u => u.Lastname == filters.Lastname);
        }

        if (!string.IsNullOrEmpty(filters.Institution))
        {
            predicates.Add(u => u.Teacher!.Institution == filters.Institution);
        }

        var paginatedResult = await _unitOfWork.TeacherRepository.GetPaginatedUsersTeachersAsync(pageNumber, pageSize, predicates);

        var dtoResult = new PaginatedResult<TeacherReadOnlyDTO>()
        {
            Data = _mapper.Map<List<TeacherReadOnlyDTO>>(paginatedResult.Data),
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };

        _logger.LogInformation("Retrieved {Count} teachers", dtoResult.Data.Count);
        return dtoResult;
    }

    public async Task<TeacherReadOnlyDTO> GetTeacherByIdAsync(int id)
    {
        Teacher? teacher = await _unitOfWork.TeacherRepository.GetAsync(id);

        if (teacher == null)
        {
            throw new EntityNotFoundException("Teacher", $"Teacher with id={id} not found.");
        }

        _logger.LogInformation("Teacher with id={id} was successfully found.", id);

        return _mapper.Map<TeacherReadOnlyDTO>(teacher);    
    }

    public async Task<TeacherReadOnlyDTO> UpdateTeacherAsync(int id, TeacherUpdateDTO request)
    {
        Teacher? teacher = await _unitOfWork.TeacherRepository.GetAsync(id);

        if (teacher == null)
        {
            throw new EntityNotFoundException("Teacher", $"Teacher with id={id} not found.");
        }

        User? userWithEmail = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email!);

        if (userWithEmail != null && userWithEmail.Id != teacher.UserId) 
        {
            throw new EntityAlreadyExistsException("User", "A user with this email already exists.");
        }

        Teacher? teacherWithPhone = await _unitOfWork.TeacherRepository.GetByPhoneNumberAsync(request.PhoneNumber!);


        if (teacherWithPhone != null && teacherWithPhone.Id != teacher.Id)
        {
            throw new EntityAlreadyExistsException("Teacher", "A teacher with this phone number already exists.");
        }

        teacher.User.Email = request.Email!;
        teacher.User.Firstname = request.Firstname!;
        teacher.User.Lastname = request.Lastname!;

        teacher.PhoneNumber = request.PhoneNumber!;
        teacher.Institution = request.Institution!;


        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Teacher with id={id} was successfully updated.", id);
        return _mapper.Map<TeacherReadOnlyDTO>(teacher);

    }

    public async Task DeleteTeacherAsync(int id)
    {
        Teacher? teacher = await _unitOfWork.TeacherRepository.GetAsync(id);

        if (teacher == null)
        {
            throw new EntityNotFoundException("Teacher", $"Teacher with id={id} not found.");
        }

        if (await _unitOfWork.CourseRepository.TeacherHasCoursesAsync(id))
        {
            throw new EntityConflictException("Teacher", "Teacher has courses. Reassign or delete them first.");
        }

        var now = DateTime.UtcNow;

        teacher.IsDeleted = true;
        teacher.DeletedAt = now;

        teacher.User.IsDeleted = true;
        teacher.User.DeletedAt = now;

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Teacher with id={id} has been deleted successfully", id);
    }
}
