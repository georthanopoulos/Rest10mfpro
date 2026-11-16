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
[Route("api/v1/students")]
public class StudentsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IAuthorizationService _authorizationService;

    public StudentsController(IApplicationService applicationService, IAuthorizationService authorizationService)
    {
        _applicationService = applicationService;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Pre-registers a new Student and User account. The returned activation token
    /// is used by the student at /auth/activate to choose a password.
    /// </summary>
    /// <param name="studentPreRegisterDTO">The student's registration details.</param>
    /// <returns>The created user and the one-time activation token.</returns>
    /// <response code="201">If the student was pre-registered.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to register students.</response>
    /// <response code="409">If the username, email or AM already exists.</response>
    [HttpPost]
    [Authorize(Policy = "INSERT_STUDENT")]
    [ProducesResponseType(typeof(PreRegistrationResultDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PreRegistrationResultDTO>> PreRegisterStudent(
        [FromBody] StudentPreRegisterDTO studentPreRegisterDTO)
    {
        var result = await _applicationService.StudentService.PreRegisterAsync(studentPreRegisterDTO);

        return CreatedAtAction(
            actionName: nameof(UsersController.GetUserById),
            controllerName: "Users",
            routeValues: new { id = result.User.Id },
            value: result);
    }

    /// <summary>
    /// Gets a paginated list of students with optional filtering.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page. Default is 10.</param>
    /// <param name="filters">Optional filters for username, email, lastname, am and institution.</param>
    /// <returns>A paginated list of students matching the filters.</returns>
    /// <response code="200">Returns the paginated student list.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to list students.</response>
    [HttpGet]
    [Authorize(Policy = "VIEW_STUDENTS")]
    [ProducesResponseType(typeof(PaginatedResult<StudentReadOnlyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<StudentReadOnlyDTO>>> GetStudents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] StudentFiltersDTO? filters = null)
    {
        var result = await _applicationService.StudentService
            .GetPaginatedStudentsFilteredAsync(pageNumber, pageSize, filters ?? new StudentFiltersDTO());

        return Ok(result);

    }

    /// <summary>
    /// Gets a student by id. A student can view only their own profile;
    /// users with the VIEW_STUDENT capability can view any student.
    /// </summary>
    /// <param name="id">The id of the student.</param>
    /// <returns>The student details.</returns>
    /// <response code="200">Returns the student.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user is not allowed to view this student.</response>
    /// <response code="404">If the student does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(StudentReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentReadOnlyDTO>> GetStudentById(int id)
    {
        var student = await _applicationService.StudentService.GetStudentByIdAsync(id);

        var authResult = await _authorizationService.AuthorizeAsync(
            User, student.UserId, new OwnerOrCapabilityRequirement("VIEW_STUDENT"));

        if (!authResult.Succeeded)
        {
            throw new EntityForbiddenException("Student", "You do not have permission to view this student.");
        }

        return Ok(student);
    }

    /// <summary>
    /// Updates an existing student's details (email, firstname, lastname, institution, department).
    /// </summary>
    /// <param name="id">The id of the student to update.</param>
    /// <param name="request">The new student details.</param>
    /// <returns>The updated student.</returns>
    /// <response code="200">Returns the updated student.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to edit students.</response>
    /// <response code="404">If the student does not exist.</response>
    /// <response code="409">If the email is already used by another user.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "EDIT_STUDENT")]
    [ProducesResponseType(typeof(StudentReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudentReadOnlyDTO>> UpdateStudent(int id,
        [FromBody] StudentUpdateDTO request)
    {
        var updated = await _applicationService.StudentService.UpdateStudentAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Soft deletes a student and the associated user account.
    /// </summary>
    /// <param name="id">The id of the student to delete.</param>
    /// <response code="204">If the student was deleted.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to delete students.</response>
    /// <response code="404">If the student does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DELETE_STUDENT")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        await _applicationService.StudentService.DeleteStudentAsync(id);

        return NoContent();
    }

}