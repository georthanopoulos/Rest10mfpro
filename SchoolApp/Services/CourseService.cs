using AutoMapper;
using SchoolApp.Core;
using SchoolApp.DTO;
using SchoolApp.Exceptions;
using SchoolApp.Models;
using SchoolApp.Repositories;

namespace SchoolApp.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CourseService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CourseReadOnlyDTO> InsertCourseAsync(CourseInsertDTO request)
    {
        Teacher? teacher = await GetTeacherOrThrowAsync(request.TeacherId);

        Course course = _mapper.Map<Course>(request);
        course.Teacher = teacher;

        await _unitOfWork.CourseRepository.AddAsync(course);
        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Course with id={id} was successfully created.", course.Id);
        return _mapper.Map<CourseReadOnlyDTO>(course);
    }

    public async Task<PaginatedResult<CourseReadOnlyDTO>> GetPaginatedCoursesAsync(int pageNumber, int pageSize)
    {
        var paginatedResult = await _unitOfWork.CourseRepository.GetPaginatedCoursesWithTeacherAsync(pageNumber, pageSize);

        var dtoResult = new PaginatedResult<CourseReadOnlyDTO>()
        {
            Data = _mapper.Map<List<CourseReadOnlyDTO>>(paginatedResult.Data),
            TotalRecords = paginatedResult.TotalRecords,
            PageNumber = paginatedResult.PageNumber,
            PageSize = paginatedResult.PageSize
        };

        _logger.LogInformation("Retrieved {Count} courses", dtoResult.Data.Count);
        return dtoResult;
    }

    public async Task<CourseReadOnlyDTO> GetCourseByIdAsync(int id)
    {
        Course? course = await _unitOfWork.CourseRepository.GetAsync(id);

        if (course == null)
        {
            throw new EntityNotFoundException("Course", $"Course with id={id} not found.");
        }

        _logger.LogInformation("Course with id={id} was successfully found.", id);
        return _mapper.Map<CourseReadOnlyDTO>(course);
    }

    public async Task<CourseReadOnlyDTO> UpdateCourseAsync(int id, CourseUpdateDTO request)
    {
        Course? course = await _unitOfWork.CourseRepository.GetAsync(id);

        if (course == null)
        {
            throw new EntityNotFoundException("Course", $"Course with id={id} not found.");
        }

        Teacher? teacher = await GetTeacherOrThrowAsync(request.TeacherId);

        course.Description = request.Description!;
        course.TeacherId = request.TeacherId;
        course.Teacher = teacher;

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Course with id={id} was successfully updated.", id);
        return _mapper.Map<CourseReadOnlyDTO>(course);
    }

    public async Task DeleteCourseAsync(int id)
    {
        Course? course = await _unitOfWork.CourseRepository.GetAsync(id);

        if (course == null)
        {
            throw new EntityNotFoundException("Course", $"Course with id={id} not found.");
        }

        course.IsDeleted = true;
        course.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        _logger.LogInformation("Course with id={id} has been deleted successfully.", id);
    }

                                                // Returns the teacher with the given id, or null when no teacher is given.
                                                // Throws 400 when a teacher id is given but no such (non-deleted) teacher exists.
    private async Task<Teacher?> GetTeacherOrThrowAsync(int? teacherId)
    {
        if (teacherId == null)
        {
            return null;
        }

        Teacher? teacher = await _unitOfWork.TeacherRepository.GetAsync(teacherId.Value);

        if (teacher == null)
        {
            throw new InvalidArgumentException("TeacherId", $"Teacher with id={teacherId} does not exist.");
        }

        return teacher;
    }
}