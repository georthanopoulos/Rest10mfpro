using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SchoolApp.DTO;

public class ActivateAccountDTO
{
    [Required (ErrorMessage = "The {0} field is required.")]
    [StringLength(64,MinimumLength = 64, ErrorMessage = "Invalid activation token.")]
    public string? Token { get; set; }

    [Required (ErrorMessage = "The {0} field is required.")]
    [RegularExpression(@"(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?\W)^.{8,}$",
        ErrorMessage = "Password must contain at least one uppercase, one lowercase, " +
        "one digit, and one special character")]
    [StringLength(60, ErrorMessage = "Password must not exceed 60 characters.")]
    public string? Password { get; set; }

}
