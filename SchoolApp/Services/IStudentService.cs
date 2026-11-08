using SchoolApp.DTO;

namespace SchoolApp.Services;

public interface IStudentService
{
    Task<PreRegistrationResultDTO> PreRegisterAsync(StudentPreRegisterDTO request);
}