using System.Linq.Expressions;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.UnitTests.TestDoubles;

// Gerçek EfUserRepository, EF Core'un "IsDeleted" global query filter'ı sayesinde yumuşak
// silinmiş kullanıcıları asla döndürmez — bu davranışı burada elle taklit ediyoruz ki
// testler production'daki gerçek repository davranışıyla tutarlı kalsın.
public class FakeUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public void Seed(User user)
    {
        if (user.Id == 0)
            user.Id = _nextId++;
        _users.Add(user);
    }

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => !u.IsDeleted && u.Email == email));

    public Task<User?> GetAsync(Expression<Func<User, bool>> filter) =>
        Task.FromResult(_users.Where(u => !u.IsDeleted).AsQueryable().FirstOrDefault(filter));

    public Task<List<User>> GetAllAsync(Expression<Func<User, bool>>? filter = null)
    {
        var query = _users.Where(u => !u.IsDeleted).AsQueryable();
        if (filter != null)
            query = query.Where(filter);
        return Task.FromResult(query.ToList());
    }

    public Task AddAsync(User entity)
    {
        Seed(entity);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<User> entities)
    {
        foreach (var entity in entities)
            Seed(entity);
        return Task.CompletedTask;
    }

    // Sahte depo aynı nesne referansını tuttuğu için mutasyonlar zaten yansımış oluyor.
    public Task UpdateAsync(User entity) => Task.CompletedTask;

    public Task UpdateRangeAsync(IEnumerable<User> entities) => Task.CompletedTask;

    public Task DeleteAsync(User entity)
    {
        _users.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<User> entities)
    {
        foreach (var entity in entities)
            _users.Remove(entity);
        return Task.CompletedTask;
    }
}
