using SchoolApp.Core;
using SchoolApp.Core.Filters;
using SchoolApp.DTO;

namespace SchoolApp.Services;

public interface ITeacherService
{   
    Task<PreRegistrationResultDTO> PreRegisterAsync(TeacherPreRegisterDTO request);

    Task<PaginatedResult<TeacherReadOnlyDTO>> GetPaginatedTeachersFilteredAsync(int pageNumber,
        int pageSize, TeacherFiltersDTO filters);
}