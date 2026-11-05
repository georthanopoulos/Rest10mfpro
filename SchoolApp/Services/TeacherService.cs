using AutoMapper;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;

namespace SchoolApp.Services;

public class TeacherService : ITeacherService
{

    private readonly IActivationTokenUtil _activationTokenUtil;
    private readonly IUnitOfWork _unitOfWork;
    private IMapper _mapper;
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
}
