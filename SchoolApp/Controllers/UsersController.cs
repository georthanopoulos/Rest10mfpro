using SchoolApp.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Security.ResourceAuthorization;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IAuthorizationService _authorizationService;

    public UsersController(IApplicationService applicationService, IAuthorizationService authorizationService)
    {
        _applicationService = applicationService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets a user by their ID. A user can view only their own profile;
    /// users with the VIEW_USERS capability can view any user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user details.</returns>
    /// <response code="200">Returns the requested user.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user is not allowed to view this user.</response>
    /// <response code="404">If no user exists with the given ID.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(UserReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadOnlyDTO>> GetUserById(int id)
    {
        // The resource IS the user id, so we can authorize before hitting the DB
        var authResult = await _authorizationService.AuthorizeAsync(
            User, id, new OwnerOrCapabilityRequirement(Capabilities.ViewUsers));

        if (!authResult.Succeeded)
        {
            throw new EntityForbiddenException("User", "You do not have permission to view this user.");
        }

        var user = await _applicationService.UserService.GetUserByIdAsync(id);
        return Ok(user);
    }

    /// <summary>
    /// Gets a user by their username. A user can view only their own profile;
    /// users with the VIEW_USERS capability can view any user.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <returns>The user details.</returns>
    /// <response code="200">Returns the requested user.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user is not allowed to view this user (also returned
    /// to callers without VIEW_USERS when the username does not exist, to prevent username enumeration).</response>
    /// <response code="404">If no user exists with the given username (only for callers with VIEW_USERS).</response>
    [HttpGet("by-username/{username}")]
    [Authorize]
    [ProducesResponseType(typeof(UserReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserReadOnlyDTO>> GetUserByUsername(string username)
    {
        UserReadOnlyDTO user;

        try
        {
            // We only know the username, so we need the user first to get the owner id
            user = await _applicationService.UserService.GetUserByUsernameAsync(username);
        }
        catch (EntityNotFoundException)
        {
            // Callers who may view any user get the honest 404.
            // Everyone else gets 403, so they cannot learn which usernames exist (preventing from Brute force attack type)..
            if (User.HasClaim("Capability", Capabilities.ViewUsers))
            {
                throw;  // re-throws the aforementioned EntityNotFoundException. 
            }

            throw new EntityForbiddenException("User", "You do not have permission to view this user.");
        }

        var authResult = await _authorizationService.AuthorizeAsync(
            User, user.Id, new OwnerOrCapabilityRequirement(Capabilities.ViewUsers));

        if (!authResult.Succeeded)
        {
            throw new EntityForbiddenException("User", "You do not have permission to view this user.");
        }

        return Ok(user);
    }

    /// <summary>
    /// Gets a paginated list of users with optional filtering.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page. Default is 10.</param>
    /// <param name="filters">Optional filters for username, email, and role.</param>
    /// <returns>A paginated list of users matching the filters.</returns>
    /// <response code="200">Returns the paginated user list.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to list users.</response>
    [HttpGet]
    [Authorize(Policy = Capabilities.ViewUsers)]
    [ProducesResponseType(typeof(PaginatedResult<UserReadOnlyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<UserReadOnlyDTO>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserFiltersDTO? filters = null)
    {
        var result = await _applicationService.UserService
            .GetPaginatedUsersFilteredAsync(pageNumber, pageSize, filters ?? new UserFiltersDTO());

        return Ok(result);
    }
}