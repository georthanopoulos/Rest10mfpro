using SchoolApp.DTO;

namespace SchoolApp.Services;

public interface ITeacherService
{   
    Task<PreRegistrationResultDTO> PreRegisterAsync(TeacherPreRegisterDTO request);
}