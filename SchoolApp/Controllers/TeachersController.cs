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
    /// Pre-registers a new Teacher and User account
    /// </summary>
    /// <param name="teacherPreRegisterDTO"></param>
    /// <returns>Activation Token</returns>
    [HttpPost]
    [Authorize(Policy = "INSERT_TEACHER")]
    [ProducesResponseType(typeof(PreRegistrationResultDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PreRegistrationResultDTO>> PreRegisterTeacher(
       [FromBody] TeacherPreRegisterDTO teacherPreRegisterDTO)                             // [FromBody] -> is by default but just for readability purposes
    {
        var result = await _applicationService.TeacherService.PreRegisterAsync(teacherPreRegisterDTO);

        return CreatedAtAction(
            actionName: nameof(UsersController.GetUserById),
            controllerName: "Users",
            routeValues: new { id = result.User.Id },
            value: result);
    }
}
