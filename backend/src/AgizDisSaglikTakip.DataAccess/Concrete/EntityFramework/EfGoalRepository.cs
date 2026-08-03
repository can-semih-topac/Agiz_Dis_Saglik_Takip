using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfGoalRepository : EfRepositoryBase<Goal>, IGoalRepository
{
    public EfGoalRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Goal>> GetByUserIdAsync(int userId)
    {
        return await Context.Goals
            .Where(g => g.UserId == userId)
            .ToListAsync();
    }
}
