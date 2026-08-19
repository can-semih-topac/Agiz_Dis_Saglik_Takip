using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfGoalStatusRepository : EfRepositoryBase<GoalStatus>, IGoalStatusRepository
{
    public EfGoalStatusRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<GoalStatus>> GetByGoalIdAsync(int goalId)
    {
        return await Context.GoalStatuses
            .Where(gs => gs.GoalId == goalId)
            .ToListAsync();
    }

    public async Task<List<GoalStatus>> GetLast7DaysByUserIdAsync(int userId)
    {
        var sevenDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

        return await Context.GoalStatuses
            .Include(gs => gs.Goal)
            .Where(gs => gs.Goal.UserId == userId && gs.ActivityDate >= sevenDaysAgo)
            .ToListAsync();
    }

    public async Task<List<GoalStatus>> GetAllByUserIdAsync(int userId)
    {
        return await Context.GoalStatuses
            .Include(gs => gs.Goal)
            .Where(gs => gs.Goal.UserId == userId)
            .ToListAsync();
    }
}
