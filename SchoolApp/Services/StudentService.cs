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

    public async Task<PaginatedResult<StudentReadOnlyDTO>> GetPaginatedStudentsFilteredAsync(int pageNumber,
        int pageSize, StudentFiltersDTO filters)
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

        if (!string.IsNullOrEmpty(filters.Am))
        {
            predicates.Add(u => u.Student!.Am == filters.Am);
        }

        if (!string.IsNullOrEmpty(filters.Institution))
        {
            predicates.Add(u => u.Student!.Institution == filters.Institution);
        }

        var paginatedResult = await _unitOfWork.StudentRepository.GetPaginatedUsersStudentsAsync(pageNumber, pageSize, predicates);

        var dtoResult = new PaginatedResult<StudentReadOnlyDTO>()
        {
            Data = _mapper.Map<List<StudentReadOnlyDTO>>(paginatedResult.Data),
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };

        _logger.LogInformation("Retrieved {Count} students", dtoResult.Data.Count);
        return dtoResult;
    }
}