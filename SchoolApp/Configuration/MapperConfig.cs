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
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Teacher!.Id))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Teacher!.PhoneNumber))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Teacher!.Institution))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Teacher!.UserId));

        CreateMap<Teacher, TeacherReadOnlyDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Firstname, opt => opt.MapFrom(src => src.User.Firstname))
            .ForMember(dest => dest.Lastname, opt => opt.MapFrom(src => src.User.Lastname));


        CreateMap<TeacherPreRegisterDTO, Teacher>();

        CreateMap<StudentPreRegisterDTO, User>();

        CreateMap<StudentPreRegisterDTO, Student>();

        CreateMap<User, StudentReadOnlyDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Student!.Id))
            .ForMember(dest => dest.Am, opt => opt.MapFrom(src => src.Student!.Am))
            .ForMember(dest => dest.Institution, opt => opt.MapFrom(src => src.Student!.Institution))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Student!.Department))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Student, StudentReadOnlyDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Firstname, opt => opt.MapFrom(src => src.User.Firstname))
            .ForMember(dest => dest.Lastname, opt => opt.MapFrom(src => src.User.Lastname));

    }
}