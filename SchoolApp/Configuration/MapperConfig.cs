using AutoMapper;
using SchoolApp.DTO;
using SchoolApp.Models;

namespace SchoolApp.Configuration;

public class MapperConfig : Profile
{

    public MapperConfig()
    {
        CreateMap<User, UserReadOnlyDTO>()        // <source, destination>
            .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.Role.Name));   // When there is a different name between the src and a dest place.

        CreateMap<TeacherPreRegisterDTO, User>();

        CreateMap<TeacherPreRegisterDTO, Teacher>();

    }
}