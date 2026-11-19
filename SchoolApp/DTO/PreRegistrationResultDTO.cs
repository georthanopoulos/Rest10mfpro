namespace SchoolApp.DTO;

public class PreRegistrationResultDTO
{
    public UserReadOnlyDTO User { get; set; } = null!;
    public string ActivationToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }

}
