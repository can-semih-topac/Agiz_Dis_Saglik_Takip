using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfGoalPauseRepository : EfRepositoryBase<GoalPause>, IGoalPauseRepository
{
    public EfGoalPauseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<GoalPause?> GetActivePauseAsync(int goalId)
    {
        return await Context.GoalPauses
            .SingleOrDefaultAsync(gp => gp.GoalId == goalId && gp.EndDate == null);
    }

    public async Task<List<GoalPause>> GetAllByUserIdAsync(int userId)
    {
        return await Context.GoalPauses
            .Include(gp => gp.Goal)
            .Where(gp => gp.Goal.UserId == userId)
            .ToListAsync();
    }
}
