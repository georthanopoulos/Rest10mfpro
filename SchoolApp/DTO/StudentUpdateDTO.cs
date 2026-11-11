using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTO;

public class StudentUpdateDTO
{
    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, ErrorMessage = "Email must not exceed 50 characters.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Firstname must be between 2 and 50 characters.")]
    public string? Firstname { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Lastname must be between 2 and 50 characters.")]
    public string? Lastname { get; set; }

    [Required(ErrorMessage = "The {0} field is required.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Institution must be between 2 and 50 characters.")]
    public string? Institution { get; set; }

    [StringLength(50, ErrorMessage = "Department must not exceed 50 characters.")]
    public string? Department { get; set; }
}