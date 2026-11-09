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

        CreateMap<User, TeacherReadOnlyDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Teacher.Id))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Teacher.PhoneNumber))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Teacher.Institution))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Teacher.UserId));

        CreateMap<TeacherPreRegisterDTO, Teacher>();

        CreateMap<StudentPreRegisterDTO, User>();

        CreateMap<StudentPreRegisterDTO, Student>();

    }
}