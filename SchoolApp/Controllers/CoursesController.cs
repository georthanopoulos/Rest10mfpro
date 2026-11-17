using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolApp.Core;
using SchoolApp.DTO;
using SchoolApp.Services;

namespace SchoolApp.Controllers;

[ApiController]
[Route("api/v1/courses")]
public class CoursesController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public CoursesController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    /// <summary>
    /// Creates a new course, optionally assigned to a teacher.
    /// </summary>
    /// <param name="request">The course description and the optional teacher id.</param>
    /// <returns>The created course.</returns>
    /// <response code="201">Returns the created course.</response>
    /// <response code="400">If the request is invalid or the teacher does not exist.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to create courses.</response>
    [HttpPost]
    [Authorize(Policy = "INSERT_COURSE")]
    [ProducesResponseType(typeof(CourseReadOnlyDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CourseReadOnlyDTO>> InsertCourse(
        [FromBody] CourseInsertDTO request)
    {
        var created = await _applicationService.CourseService.InsertCourseAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetCourseById),
            routeValues: new { id = created.Id },
            value: created);
    }

    /// <summary>
    /// Gets a paginated list of courses.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based). Default is 1.</param>
    /// <param name="pageSize">The number of items per page. Default is 10.</param>
    /// <returns>A paginated list of courses.</returns>
    /// <response code="200">Returns the paginated course list.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to list courses.</response>
    [HttpGet]
    [Authorize(Policy = "VIEW_COURSES")]
    [ProducesResponseType(typeof(PaginatedResult<CourseReadOnlyDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<CourseReadOnlyDTO>>> GetCourses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _applicationService.CourseService.GetPaginatedCoursesAsync(pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Gets a course by id.
    /// </summary>
    /// <param name="id">The id of the course.</param>
    /// <returns>The course details.</returns>
    /// <response code="200">Returns the course.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to view courses.</response>
    /// <response code="404">If the course does not exist.</response>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "VIEW_COURSE")]
    [ProducesResponseType(typeof(CourseReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseReadOnlyDTO>> GetCourseById(int id)
    {
        var course = await _applicationService.CourseService.GetCourseByIdAsync(id);
        return Ok(course);
    }

    /// <summary>
    /// Updates an existing course (description and teacher). A null teacher id removes the teacher.
    /// </summary>
    /// <param name="id">The id of the course to update.</param>
    /// <param name="request">The new course details.</param>
    /// <returns>The updated course.</returns>
    /// <response code="200">Returns the updated course.</response>
    /// <response code="400">If the request is invalid or the teacher does not exist.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to edit courses.</response>
    /// <response code="404">If the course does not exist.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "EDIT_COURSE")]
    [ProducesResponseType(typeof(CourseReadOnlyDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseReadOnlyDTO>> UpdateCourse(int id,
        [FromBody] CourseUpdateDTO request)
    {
        var updated = await _applicationService.CourseService.UpdateCourseAsync(id, request);
        return Ok(updated);
    }

    /// <summary>
    /// Soft deletes a course.
    /// </summary>
    /// <param name="id">The id of the course to delete.</param>
    /// <response code="204">If the course was deleted.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to delete courses.</response>
    /// <response code="404">If the course does not exist.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "DELETE_COURSE")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        await _applicationService.CourseService.DeleteCourseAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Enrolls a student in a course.
    /// </summary>
    /// <param name="courseId">The id of the course.</param>
    /// <param name="studentId">The id of the student to enroll.</param>
    /// <response code="204">If the student was enrolled.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to enroll students.</response>
    /// <response code="404">If the course or the student does not exist.</response>
    /// <response code="409">If the student is already enrolled in the course.</response>
    [HttpPost("{courseId:int}/students/{studentId:int}")]
    [Authorize(Policy = "ENROLL_STUDENT")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> EnrollStudent(int courseId, int studentId)
    {
        await _applicationService.CourseService.EnrollStudentAsync(courseId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Removes a student from a course.
    /// </summary>
    /// <param name="courseId">The id of the course.</param>
    /// <param name="studentId">The id of the student to remove.</param>
    /// <response code="204">If the student was removed from the course.</response>
    /// <response code="401">If the request is not authenticated.</response>
    /// <response code="403">If the user lacks permission to remove students from courses.</response>
    /// <response code="404">If the course does not exist or the student is not enrolled in it.</response>
    [HttpDelete("{courseId:int}/students/{studentId:int}")]
    [Authorize(Policy = "UNENROLL_STUDENT")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UnenrollStudent(int courseId, int studentId)
    {
        await _applicationService.CourseService.UnenrollStudentAsync(courseId, studentId);
        return NoContent();
    }
}