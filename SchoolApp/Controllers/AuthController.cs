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
    /// <returns>A JWT token for the authenticated user.</returns>
    /// <exception cref="EntityNotAuthorizedException">Thrown when the user credentials are invalid.</exception>
    [HttpPost("login")]
    [ProducesResponseType(typeof(JwtTokenDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JwtTokenDTO>> Login(
        [FromBody] UserLoginDTO userLoginDTO)
    {
        var user = await _applicationService.UserService.VerifyAndGetUserAsync(userLoginDTO)
            ?? throw new EntityNotAuthorizedException("User", "Bad Credentials");

        var token = _applicationService.UserService.CreateUserToken(user);

        return Ok(new JwtTokenDTO(token));
    }

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
            throw new EntityNotAuthorizedException("User", "Bad credentials");
        }

        await _applicationService.UserService.ChangePasswordAsync(userId, request);

        return NoContent();
    }

}
