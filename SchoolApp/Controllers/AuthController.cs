using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Services;
using System.Security.Claims;

namespace SchoolApp.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public AuthController(IApplicationService applicationService)                  // constructor with members for DI
    {
        _applicationService = applicationService;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if credentials are valid.
    /// </summary>
    /// <param name="userLoginDTO">The user login credentials.</param>
    /// <returns>A JWT token for the authenticated user, plus a flag telling whether the user must change the password before using the API.</returns>
    /// <response code="200">Returns the JWT token and the mustChangePassword flag.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="401">If the credentials are invalid or the account is not active.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwtTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JwtTokenDTO>> Login(
        [FromBody] UserLoginDTO userLoginDTO)
    {
        var user = await _applicationService.UserService.VerifyAndGetUserAsync(userLoginDTO);

        var token = _applicationService.UserService.CreateUserToken(user);

        string? message = user.MustChangePassword
        ? "You must change your temporary password. Use POST /api/v1/auth/change-password before using any other endpoint."
        : null;

        return Ok(new JwtTokenDTO(token, user.MustChangePassword, message));
    }

    /// <summary>
    /// Activates a pre-registered account: the user chooses a password
    /// using the one-time activation token received from the registrar.
    /// </summary>
    /// <param name="activateAccountDTO">The activation token and the new password.</param>
    /// <response code="204">If the account was activated.</response>
    /// <response code="400">If the request body is invalid or the token is invalid, used or expired.</response>
    [HttpPost("activate")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Activate(
        [FromBody] ActivateAccountDTO activateAccountDTO)
    {
        await _applicationService.UserService.ActivateAccountAsync(activateAccountDTO);

        return NoContent();
    }

    /// <summary>
    /// Changes the password of the authenticated user.
    /// Together with login, this is the only endpoint allowed while a password change is required.
    /// </summary>
    /// <remarks>
    /// If the account had a temporary password, log in again after a successful change to get a token without the password-change restriction.
    /// </remarks>
    /// <param name="request">The current and the new password.</param>
    /// <response code="204">If the password was changed.</response>
    /// <response code="400">If the request body is invalid, the current password is wrong
    /// or the new password equals the current one.</response>
    /// <response code="401">If the request is not authenticated.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordDTO request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            throw new EntityNotAuthorizedException("User", "Bad Credentials");
        }

        await _applicationService.UserService.ChangePasswordAsync(userId, request);

        return NoContent();
    }
}