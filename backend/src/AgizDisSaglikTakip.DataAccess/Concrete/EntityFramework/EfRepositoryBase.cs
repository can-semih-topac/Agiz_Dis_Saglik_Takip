using System.Linq.Expressions;
using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfRepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;

    public EfRepositoryBase(AppDbContext context)
    {
        Context = context;
    }

    public async Task<T?> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await Context.Set<T>().SingleOrDefaultAsync(filter);
    }

    public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
    {
        var query = Context.Set<T>().AsQueryable();
        if (filter != null)
            query = query.Where(filter);

        return await query.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await Context.Set<T>().AddRangeAsync(entities);
        await Context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities)
    {
        Context.Set<T>().UpdateRange(entities);
        await Context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        Context.Set<T>().Remove(entity);
        await Context.SaveChangesAsync();
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        Context.Set<T>().RemoveRange(entities);
        await Context.SaveChangesAsync();
    }
}
