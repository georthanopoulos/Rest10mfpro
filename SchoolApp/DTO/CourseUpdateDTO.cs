using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTO;

public class CourseUpdateDTO
{

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 50 characters.")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a positive number.")]
    public int? TeacherId { get; set; }
}

