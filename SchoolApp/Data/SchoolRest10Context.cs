using Microsoft.EntityFrameworkCore;
using SchoolApp.Models;

namespace SchoolApp.Data;

public class SchoolRest10Context : DbContext
{
    public SchoolRest10Context(DbContextOptions<SchoolRest10Context> options) : base(options)
    { }

    public DbSet<Teacher> Teachers { get; set; }

    public DbSet<Student> Students { get; set; }

    public DbSet<Course> Courses { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<Capability> Capabilities { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(e => e.Institution).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(t => t.User)
            .WithOne(p => p.Teacher)
            .HasForeignKey<Teacher>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade)                      // It is already the default choice.
            .HasConstraintName("FK_Teachers_Users_UserId");

            entity.HasIndex(e => e.Institution, "IX_Teachers_Institution");
            entity.HasIndex(e => e.UserId, "UQ_Teachers_UserId").IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.Am).HasMaxLength(10);
            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.Institution).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Students_Users_UserId");

            entity.HasIndex(e => e.Am, "UQ_Students_Am").IsUnique();
            entity.HasIndex(e => e.Institution, "IX_Students_Institution");
            entity.HasIndex(e => e.UserId, "UQ_Students_UserId").IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(50);

            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.Courses)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK_Courses_Teachers_TeacherId");

            // Τα πεδία του Composite Key ονομάζονται by default StudentsId και CoursesId
            entity.HasMany(d => d.Students).WithMany(p => p.Courses).UsingEntity("StudentsCourses");

            // Αν θέλουμε explicit ονομασίες πεδίων:
            //entity.HasMany(d => d.Students).WithMany(p => p.Courses)
            //    .UsingEntity("StudentsCourses",
            //        l => l.HasOne(typeof(Student)).WithMany().HasForeignKey("StudentId"),
            //        r => r.HasOne(typeof(Course)).WithMany().HasForeignKey("CourseId"));

            entity.HasIndex(e => e.Description, "IX_Courses_Description");
            entity.HasIndex(e => e.TeacherId, "IX_Courses_TeacherId");
        });

        modelBuilder.Entity<Role>(entity =>
        {
        entity.Property(e => e.Name).HasMaxLength(50);

        entity.HasMany(d => d.Capabilities).WithMany(p => p.Roles)
            .UsingEntity("RolesCapabilities", j =>         // for the first term of the complex element RoleCapabilities it has index automatically. BUT for the second term (capabilities) we need to define one.
            {
                j.HasIndex("CapabilitiesId").HasDatabaseName("IX_RolesCapabilities_CapabilityId");
                j.HasData(new { RolesId = 3, CapabilitiesId = 6 });        // TEACHER (3): view only teacher
                j.HasData(new { RolesId = 4, CapabilitiesId = 12 });       // STUDENT (4): view only student
                j.HasData(                                                 // EMPLOYEE (2): insert teacher/student, view teachers/students/courses, view users
                new { RolesId = 2, CapabilitiesId = 1 },
                new { RolesId = 2, CapabilitiesId = 2 },
                new { RolesId = 2, CapabilitiesId = 3 },
                new { RolesId = 2, CapabilitiesId = 7 },
                new { RolesId = 2, CapabilitiesId = 8 },
                new { RolesId = 2, CapabilitiesId = 9 },
                new { RolesId = 2, CapabilitiesId = 14 },
                new { RolesId = 2, CapabilitiesId = 15 },
                new { RolesId = 2, CapabilitiesId = 18 });
                j.HasData(Enumerable.Range(1, 18)                           // ADMIN (1): ALL 18 rights reserved. The command produces all numbers from 1 up to 18 and with Select a line for each one is created, instead of writing 18 line sof code.
                       .Select(id => new { RolesId = 1, CapabilitiesId = id }));

            });

        entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

        entity.HasData(new Role { Id = 1, Name = "ADMIN" },
            new Role { Id = 2, Name = "EMPLOYEE"},
            new Role { Id = 3, Name = "TEACHER" },
            new Role { Id = 4, Name = "STUDENT"});
        });

        modelBuilder.Entity<Capability>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasIndex(e => e.Name, "UQ_Capabilities_Name").IsUnique();

            entity.HasData(
            new Capability { Id = 1, Name = "INSERT_TEACHER", Description = "Create a new teacher" },
            new Capability { Id = 2, Name = "VIEW_TEACHERS", Description = "View teacher list and details" },
            new Capability { Id = 3, Name = "VIEW_TEACHER", Description = "View teacher" },
            new Capability { Id = 4, Name = "EDIT_TEACHER", Description = "Modify existing teacher" },
            new Capability { Id = 5, Name = "DELETE_TEACHER", Description = "Remove a teacher" },
            new Capability { Id = 6, Name = "VIEW_ONLY_TEACHER", Description = "View only own teacher details" },
            new Capability { Id = 7, Name = "INSERT_STUDENT", Description = "Create a new student" },
            new Capability { Id = 8, Name = "VIEW_STUDENTS", Description = "View student list and details" },
            new Capability { Id = 9, Name = "VIEW_STUDENT", Description = "View student" },
            new Capability { Id = 10, Name = "EDIT_STUDENT", Description = "Modify existing student" },
            new Capability { Id = 11, Name = "DELETE_STUDENT", Description = "Remove a student" },
            new Capability { Id = 12, Name = "VIEW_ONLY_STUDENT", Description = "View only own student details" },
            new Capability { Id = 13, Name = "INSERT_COURSE", Description = "Create a new course" },
            new Capability { Id = 14, Name = "VIEW_COURSES", Description = "View course list and details" },
            new Capability { Id = 15, Name = "VIEW_COURSE", Description = "View course" },
            new Capability { Id = 16, Name = "EDIT_COURSE", Description = "Modify existing course" },
            new Capability { Id = 17, Name = "DELETE_COURSE", Description = "Remove a course" },
            new Capability { Id = 18, Name = "VIEW_USERS", Description = "View the list of all users (registry)" });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.Lastname).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(60);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade)  
                .HasConstraintName("FK_Users_Roles_RoleId");

            entity.HasIndex(e => e.Email, "UQ_Users_Email").IsUnique();
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");
            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)            // if the client doesn't wait or closes earlier an action without awaiting for the respose then the cancelletiontoken closes the connection. New C# feature.
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(ct);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.InsertedAt = now;
                    entry.Entity.ModifiedAt = now;
                    break;
                                                         // My note: On soft delete, in order for the deleted records to be hidden from the queries the cmd: HasQueryFilter(e => !e.IsDeleted) in OnModelCreating is needed!!! Otherwise, without this the deleted records will be stil visible.
                //case EntityState.Modified:
                //    entry.Entity.ModifiedAt = now;
                //    break;

                //case EntityState.Deleted:
                //    entry.State = EntityState.Unchanged;
                //    entry.Entity.IsDeleted = true;
                //    entry.Entity.DeletedAt = now;
                //    entry.Entity.ModifiedAt = now;
                //    break;
            }
        }
    }
}

