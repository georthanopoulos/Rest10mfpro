namespace SchoolApp.DTO;

public record JwtTokenDTO(string Token, bool MustChangePassword, string? Message = null);