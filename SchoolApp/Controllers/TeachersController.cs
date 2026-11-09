using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Core;
using SchoolApp.Core.Filters;
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

    /// <summary>
    /// Gets a paginated list of teachers with optional filtering.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page. Default is 10.</param>
    /// <param name="teacherFiltersDTO">Optional filters for username, email, lastname and institution.</param>
    /// <returns>A paginated list of teaches matching the filters.</returns>
    /// <response code="200">Returns the paginated teacher list.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to list teachers.</response>
    [HttpGet]
    [Authorize(Policy = "VIEW_TEACHERS")]
    [ProducesResponseType(typeof(PaginatedResult<TeacherReadOnlyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<TeacherReadOnlyDTO>>> GetTeachers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TeacherFiltersDTO? teacherFiltersDTO = null)
    {
        var result = await _applicationService.TeacherService
            .GetPaginatedTeachersFilteredAsync(pageNumber, pageSize, teacherFiltersDTO ?? new TeacherFiltersDTO());

        return Ok(result);
    }

}
