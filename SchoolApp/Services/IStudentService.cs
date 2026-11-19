using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;

namespace SchoolApp.Services;

public interface IStudentService
{
    Task<PreRegistrationResultDTO> PreRegisterAsync(StudentPreRegisterDTO request);

    Task<PaginatedResult<StudentReadOnlyDTO>> GetPaginatedStudentsFilteredAsync(int pageNumber,
        int pageSize, StudentFiltersDTO filters);

    Task<StudentReadOnlyDTO> GetStudentByIdAsync(int id);

    Task<StudentReadOnlyDTO> UpdateStudentAsync(int id, StudentUpdateDTO request);

    Task DeleteStudentAsync(int id);
}