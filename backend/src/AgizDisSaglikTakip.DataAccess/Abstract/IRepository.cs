using System.Linq.Expressions;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(Expression<Func<T, bool>> filter);
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
    Task AddAsync(T entity);
    // Çok sayıda kaydı tek SaveChanges ile eklemek için (ör. demo hesabı klonlama) — AddAsync'in
    // her çağrıda ayrı SaveChanges yapması toplu işlemlerde ciddi performans kaybına yol açıyor.
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
