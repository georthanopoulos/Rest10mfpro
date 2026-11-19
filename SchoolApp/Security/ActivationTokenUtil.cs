using System.Security.Cryptography;
using System.Text;

namespace SchoolApp.Security;

public class ActivationTokenUtil : IActivationTokenUtil
{
    public string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));   // 64 hex chars, cryptographically random
    }

    public string Hash(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));   // 64 hex chars, same length as the TokenHash column
    }
}
