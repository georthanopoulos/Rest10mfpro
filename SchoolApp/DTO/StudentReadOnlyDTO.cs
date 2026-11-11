namespace SchoolApp.DTO;

public class StudentReadOnlyDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Am { get; set; } = null!;
    public string Institution { get; set; } = null!;
    public string? Department { get; set; }
    public int UserId { get; set; }
}