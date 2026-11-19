namespace SchoolApp.DTO;

public class CourseReadOnlyDTO
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public int? TeacherId { get; set; }
    public string? TeacherLastname { get; set; }
}
