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
[Route("api/v1/teachers")]
public class TeachersController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IAuthorizationService _authorizationService;

    public TeachersController(IApplicationService applicationService, IAuthorizationService authorizationService)
    {
        _applicationService = applicationService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Pre-registers a new Teacher and User account. The returned activation token
    /// is used by the teacher at /auth/activate to choose a password.
    /// </summary>
    /// <param name="teacherPreRegisterDTO">The teacher's registration details.</param>
    /// <returns>The created user and the one-time activation token.</returns>
    /// <response code="201">If the teacher was pre-registered.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to register teachers.</response>
    /// <response code="409">If the username, email or phone number already exists.</response>
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
    /// <param name="filters">Optional filters for username, email, lastname and institution.</param>
    /// <returns>A paginated list of teachers matching the filters.</returns>
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
        [FromQuery] TeacherFiltersDTO? filters = null)
    {
        var result = await _applicationService.TeacherService
            .GetPaginatedTeachersFilteredAsync(pageNumber, pageSize, filters ?? new TeacherFiltersDTO());

        return Ok(result);
    }

    /// <summary>
    /// Gets a teacher by id. A teacher can view only their own profile;
    /// users with the VIEW_TEACHER capability can view any teacher.
    /// </summary>
    /// <param name="id">The id of the teacher.</param>
    /// <returns>The teacher details.</returns>
    /// <response code="200">Returns the teacher.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user is not allowed to view this teacher.</response>
    /// <response code="404">If the teacher does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(TeacherReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherReadOnlyDTO>> GetTeacherById(int id)
    {
        var teacher = await _applicationService.TeacherService.GetTeacherByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(
            User, teacher.UserId, new OwnerOrCapabilityRequirement("VIEW_TEACHER"));

        if (!authResult.Succeeded)
        {
            throw new EntityForbiddenException("Teacher", "You do not have permission to view this teacher.");
        }

        return Ok(teacher);
    }

    /// <summary>
    /// Gets the courses taught by a teacher. A teacher can view only their own courses;
    /// users with the VIEW_COURSES capability can view the courses of any teacher.
    /// </summary>
    /// <param name="id">The id of the teacher.</param>
    /// <param name="pageNumber">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page. Default is 10.</param>
    /// <returns>A paginated list of the teacher's courses.</returns>
    /// <response code="200">Returns the paginated course list.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user is not allowed to view the courses of this teacher.</response>
    /// <response code="404">If the teacher does not exist.</response>
    [HttpGet("{id:int}/courses")]
    [Authorize]
    [ProducesResponseType(typeof(PaginatedResult<CourseReadOnlyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResult<CourseReadOnlyDTO>>> GetTeacherCourses(int id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var teacher = await _applicationService.TeacherService.GetTeacherByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(
            User, teacher.UserId, new OwnerOrCapabilityRequirement("VIEW_COURSES"));

        if (!authResult.Succeeded)
        {
            throw new EntityForbiddenException("Teacher", "You do not have permission to view the courses of this teacher.");
        }

        var result = await _applicationService.CourseService
            .GetPaginatedCoursesByTeacherAsync(id, pageNumber, pageSize);

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing teacher's details (email, firstname, lastname, phone number, institution).
    /// </summary>
    /// <param name="id">The id of the teacher to update.</param>
    /// <param name="request">The new teacher details.</param>
    /// <returns>The updated teacher.</returns>
    /// <response code="200">Returns the updated teacher.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to edit teachers.</response>
    /// <response code="404">If the teacher does not exist.</response>
    /// <response code="409">If the email or phone number is already used by another teacher.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "EDIT_TEACHER")]
    [ProducesResponseType(typeof(TeacherReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TeacherReadOnlyDTO>> UpdateTeacher(int id,
        [FromBody] TeacherUpdateDTO request)
    {
        var updated = await _applicationService.TeacherService.UpdateTeacherAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Soft deletes a teacher and the associated user account.
    /// </summary>
    /// <param name="id">The id of the teacher to delete.</param>
    /// <response code="204">If the teacher was deleted.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to delete teachers.</response>
    /// <response code="404">If the teacher does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DELETE_TEACHER")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTeacher(int id)
    {
        await _applicationService.TeacherService.DeleteTeacherAsync(id);
        return NoContent();
    }
}
