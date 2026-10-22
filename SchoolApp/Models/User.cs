namespace SchoolApp.Models;

public class User : BaseEntity
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public int RoleId { get; set; }
                                         // We don't hold any FKs here! We summoned the Student and Teacher as the "strong" sides. Therefore, they hold the FKs. The User is the "weak" side though.
    public Role Role { get; set; } = null!;

    public Student? Student { get; set; }

    public Teacher? Teacher { get; set; }
}
