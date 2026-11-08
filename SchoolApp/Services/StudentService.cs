using AutoMapper;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;

namespace SchoolApp.Services;

public class StudentService : IStudentService
{
    private readonly IActivationTokenUtil _activationTokenUtil;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IActivationTokenUtil activationTokenUtil,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<StudentService> logger)
    {
        _activationTokenUtil = activationTokenUtil;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PreRegistrationResultDTO> PreRegisterAsync(StudentPreRegisterDTO request)
    {
        bool exists = await _unitOfWork.UserRepository.UsernameOrEmailExistsAsync(request.Username!, request.Email!);

        if (exists)
        {
            throw new EntityAlreadyExistsException("User", "A user with this username or email already exists.");
        }

        Student? existingStudent = await _unitOfWork.StudentRepository.GetByAm(request.Am);

        if (existingStudent is not null)
        {
            throw new EntityAlreadyExistsException("Student", "A student with this Am already exists.");
        }

        Role? studentRole = await _unitOfWork.RoleRepository.GetByNameAsync("STUDENT");

        if (studentRole is null)
        {
            throw new InvalidOperationException("Role STUDENT is missing from the database.");
        }

        User user = _mapper.Map<User>(request);
        Student student = _mapper.Map<Student>(request);

        user.Role = studentRole;
        user.Status = AccountStatus.Pending;
        user.Password = null;
        user.Student = student;

        string rawToken = _activationTokenUtil.GenerateToken();

        ActivationToken activationToken = new ActivationToken
        {
            User = user,
            TokenHash = _activationTokenUtil.Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddHours(72)
        };

        await _unitOfWork.UserRepository.AddAsync(user);
        await _unitOfWork.ActivationTokenRepository.AddAsync(activationToken);

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("User with id={Id} and username={Username} pre-registered as student successfully", user.Id, user.Username);

        return new PreRegistrationResultDTO
        {
            User = _mapper.Map<UserReadOnlyDTO>(user),
            ActivationToken = rawToken,
            ExpiresAt = activationToken.ExpiresAt
        };
    }
}