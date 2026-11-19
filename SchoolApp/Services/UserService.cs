using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;
using SchoolApp.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace SchoolApp.Services;

public class UserService : IUserService
{
    private readonly IEncryptionUtil _encryptionUtil;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IActivationTokenUtil _activationTokenUtil;

    public UserService(IEncryptionUtil encryptionUtil, IUnitOfWork unitOfWork,
        IMapper mapper, ILogger<UserService> logger, IConfiguration configuration, IActivationTokenUtil activationTokenUtil)
    {
        _encryptionUtil = encryptionUtil;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _activationTokenUtil = activationTokenUtil;
    }

    public async Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersFilteredAsync(int pageNumber, int pageSize,
        UserFiltersDTO filters)
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

        if (!string.IsNullOrEmpty(filters.UserRole))
        {
            string[] validRoles = { "ADMIN", "EMPLOYEE", "TEACHER", "STUDENT" };

            if (!validRoles.Contains(filters.UserRole))
            { 
                throw new InvalidArgumentException("UserRole",
                    $"Unknown role '{filters.UserRole}'. Valid roles: {string.Join(", ", validRoles)}.");
            }

            predicates.Add(u => u.Role.Name == filters.UserRole);
        }

        var paginatedResult = await _unitOfWork.UserRepository.GetUsersAsync(pageNumber, pageSize, predicates);

        var dtoResult = new PaginatedResult<UserReadOnlyDTO>()
        {
            Data = _mapper.Map<List<UserReadOnlyDTO>>(paginatedResult.Data),
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };

        _logger.LogInformation("Retrieved {Count} users", dtoResult.Data.Count);
        return dtoResult;
    }

    public async Task<UserReadOnlyDTO> GetUserByUsernameAsync(string username)
    {
        User? user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

        if (user is null)
        {
            throw new EntityNotFoundException("User", "Entity not found.");
        }

        _logger.LogInformation("User found with username={Username}", username);
        return _mapper.Map<UserReadOnlyDTO>(user);
    }

    public async Task<UserReadOnlyDTO> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.UserRepository.GetAsync(id);

        if (user == null)
        {
            throw new EntityNotFoundException("User", $"User with id {id} not found");
        }

        _logger.LogInformation("User with id {Id} found", id);
        return _mapper.Map<UserReadOnlyDTO>(user);
    }

    // Used only to burn the same CPU time when the user does not exist
    private static readonly string DummyPasswordHash = BCrypt.Net.BCrypt.HashPassword("dummy-password");

    public async Task<User> VerifyAndGetUserAsync(UserLoginDTO credentials)
    {
        User? user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(credentials.Username!);

        // Always run one password check, so the response time does not reveal
        // whether the username exists
        string hashToCheck = user?.Password ?? DummyPasswordHash;
        bool isPasswordValid = _encryptionUtil.IsValidPassword(credentials.Password!, hashToCheck);

        if (user is null
            || user.Status != AccountStatus.Active
            || user.Password is null
            || !isPasswordValid)
        {
            throw new EntityNotAuthorizedException("Auth", "Bad Credentials");
        }

        _logger.LogInformation("User with username={Username} verified for login.", credentials.Username);
        return user;
    }

    public async Task ActivateAccountAsync(ActivateAccountDTO request)
    {
        string tokenHash = _activationTokenUtil.Hash(request.Token!);

        ActivationToken? activationToken = await _unitOfWork.ActivationTokenRepository.GetByHashAsync(tokenHash);

        if (activationToken is null
            || activationToken.UsedAt is not null
            || activationToken.ExpiresAt < DateTime.UtcNow
            || activationToken.User.Status != AccountStatus.Pending)
        {
            throw new InvalidArgumentException("ActivationToken", "Invalid or expired activation token.");
        }

        activationToken.User.Password = _encryptionUtil.Encrypt(request.Password!);
        activationToken.User.Status = AccountStatus.Active;
        activationToken.UsedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("User with id={Id} activated the account.", activationToken.User.Id);
    }

    public string CreateUserToken(User user)
    {
        var secretKey = _configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsInfo = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim("MustChangePassword", user.MustChangePassword.ToString())

        };

        claimsInfo.AddRange(user.Role.Capabilities
                .Select(c => new Claim("Capability", c.Name)));

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claimsInfo,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: signingCredentials
        );

        // Serialize the token to a string
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    
    public async Task ChangePasswordAsync(int userId, ChangePasswordDTO request)
    {
        User? user = await _unitOfWork.UserRepository.GetAsync(userId);

        if (user == null || user.Status != AccountStatus.Active || user.Password == null)
        {
            throw new EntityNotAuthorizedException("User", "Bad Credentials");
        }

        if (!_encryptionUtil.IsValidPassword(request.CurrentPassword!, user.Password))
        {
            throw new InvalidArgumentException("CurrentPassword", "Current password is incorrect.");
        }

        if (_encryptionUtil.IsValidPassword(request.NewPassword!, user.Password))
        {
            throw new InvalidArgumentException("NewPassword", "New password must differ from the current one.");
        }

        user.Password = _encryptionUtil.Encrypt(request.NewPassword!);
        user.MustChangePassword = false;

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Password has changed for user with id={id}.", userId);
    }
}