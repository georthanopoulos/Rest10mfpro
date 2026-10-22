namespace SchoolApp.Models;

public class Capability
{
    public int Id { get; set; }                 // No BaseEntity extends - as User. See justification.

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<Role> Roles { get; set; } = new HashSet<Role>();


}
