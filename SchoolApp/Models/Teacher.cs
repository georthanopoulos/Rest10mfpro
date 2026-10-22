namespace SchoolApp.Models
{
    public class Teacher : BaseEntity
    {
        public int Id { get; set; } 

        public string Institution { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;           // it is not nullable so the framework cascade-delete feature provided if we delete teacher the user will also be deleted.  If it was nullable (?) then the user would be set to null!

        public ICollection<Course> Courses { get; set; } = new HashSet<Course>();   // HashSet is recommented. not list! It has quicker seek time and it also does not allow duplicates.
    }
}
