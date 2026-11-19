using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;
using SchoolApp.Models;

namespace SchoolApp.Services;

public interface IUserService
{
    Task<User> VerifyAndGetUserAsync(UserLoginDTO credentials);

    Task<UserReadOnlyDTO> GetUserByUsernameAsync(string username);

    Task<UserReadOnlyDTO> GetUserByIdAsync (int id);

    Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersFilteredAsync(int pageNumber, int pageSize,
        UserFiltersDTO filters);

    Task ActivateAccountAsync(ActivateAccountDTO request);

    string CreateUserToken(User user);

    Task ChangePasswordAsync(int userId, ChangePasswordDTO request);

}