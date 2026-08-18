using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfAdminActionLogRepository : EfRepositoryBase<AdminActionLog>, IAdminActionLogRepository
{
    public EfAdminActionLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<AdminActionLog>> GetRecentAsync(int count)
    {
        return await Context.AdminActionLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
