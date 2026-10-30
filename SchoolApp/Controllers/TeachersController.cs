using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.DTO;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[ApiController]
[Route("api/v1/teachers")]
public class TeachersController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public TeachersController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    /// <summary>
    /// Registers a new Teacher and User account
    /// </summary>
    /// <param name="teacherSignupDTO"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Policy = "INSERT_TEACHER")]
    [ProducesResponseType(typeof(UserReadOnlyDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserReadOnlyDTO>> RegisterTeacher(
       [FromBody] TeacherSignupDTO teacherSignupDTO)                             // [FromBody] -> is by default but just for readability purposes
    {
        var createdUser = await _applicationService.TeacherService.SignUpUserAsync(teacherSignupDTO);

        return CreatedAtAction(
            actionName: nameof(UsersController.GetUserById),
            controllerName: "Users",
            routeValues: new { id = createdUser.Id },
            value: createdUser);
    }
}
