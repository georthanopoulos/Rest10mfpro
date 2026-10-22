namespace SchoolApp.Models;

public class Role
{
    public int Id { get; set; }                         // BaseEntity extends - IS NOT NEEDED here! We don't need auditing! We could do it though, it is best practice. We didn't include it here for easiness on the procedures....

    public string Name { get; set; } = null!;

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public ICollection<Capability> Capabilities { get; set; } = new HashSet<Capability>();

}
