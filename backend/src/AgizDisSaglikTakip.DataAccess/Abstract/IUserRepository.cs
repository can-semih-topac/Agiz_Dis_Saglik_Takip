using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
