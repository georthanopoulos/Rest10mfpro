using SchoolApp.Models;

namespace SchoolApp.Repositories;

public interface IActivationTokenRepository : IBaseRepository<ActivationToken>
{
    Task<ActivationToken?> GetByHashAsync(string tokenHash);
}
