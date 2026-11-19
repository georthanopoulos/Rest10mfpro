namespace SchoolApp.Security;

public interface IActivationTokenUtil
{
    string GenerateToken();
    string Hash(string token);
}
