using System.Linq.Expressions;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.UnitTests.TestDoubles;

// AuthManager'ın refresh token rotasyonu/çalıntı tespiti çok adımlı, durum bağımlı (stateful)
// bir akış (ekle -> ara -> güncelle -> tekrar ara) — bunu Moq ile ayrı ayrı Setup'larla
// taklit etmek kırılgan olurdu, bu yüzden basit, gerçek durumu tutan bir sahte depo kullanıyoruz.
public class FakeRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = new();
    private int _nextId = 1;

    public IReadOnlyList<RefreshToken> All => _tokens;

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash) =>
        Task.FromResult(_tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task RevokeAllActiveForUserAsync(int userId)
    {
        foreach (var token in _tokens.Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow))
            token.RevokedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task DeleteExpiredForUserAsync(int userId)
    {
        _tokens.RemoveAll(t => t.UserId == userId && t.ExpiresAt <= DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetAsync(Expression<Func<RefreshToken, bool>> filter) =>
        Task.FromResult(_tokens.AsQueryable().FirstOrDefault(filter));

    public Task<List<RefreshToken>> GetAllAsync(Expression<Func<RefreshToken, bool>>? filter = null)
    {
        var query = _tokens.AsQueryable();
        if (filter != null)
            query = query.Where(filter);
        return Task.FromResult(query.ToList());
    }

    public Task AddAsync(RefreshToken entity)
    {
        entity.Id = _nextId++;
        _tokens.Add(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<RefreshToken> entities)
    {
        foreach (var entity in entities)
        {
            entity.Id = _nextId++;
            _tokens.Add(entity);
        }
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RefreshToken entity) => Task.CompletedTask;

    public Task UpdateRangeAsync(IEnumerable<RefreshToken> entities) => Task.CompletedTask;

    public Task DeleteAsync(RefreshToken entity)
    {
        _tokens.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<RefreshToken> entities)
    {
        foreach (var entity in entities)
            _tokens.Remove(entity);
        return Task.CompletedTask;
    }
}
