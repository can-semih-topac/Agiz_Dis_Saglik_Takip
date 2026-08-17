using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfLogRepository : EfRepositoryBase<Log>, ILogRepository
{
    public EfLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Log>> GetRecentAsync(int count)
    {
        return await Context.Logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
