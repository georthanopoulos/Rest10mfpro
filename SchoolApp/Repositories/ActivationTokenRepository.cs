using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories;

public class ActivationTokenRepository : BaseRepository<ActivationToken>, IActivationTokenRepository
{
    public ActivationTokenRepository(SchoolRest10Context context) : base(context)
    {
    }

    public async Task<ActivationToken?> GetByHashAsync(string tokenHash)
    {
        return await _context.ActivationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }
}
