using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfRefreshTokenRepository : EfRepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public EfRefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await Context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task RevokeAllActiveForUserAsync(int userId)
    {
        var activeTokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (activeTokens.Count == 0)
            return;

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        await Context.SaveChangesAsync();
    }

    public async Task DeleteExpiredForUserAsync(int userId)
    {
        var expiredTokens = await Context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Count == 0)
            return;

        Context.RefreshTokens.RemoveRange(expiredTokens);
        await Context.SaveChangesAsync();
    }
}
