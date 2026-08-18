using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfStatusNoteRepository : EfRepositoryBase<StatusNote>, IStatusNoteRepository
{
    public EfStatusNoteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<StatusNote>> GetLast7DaysByUserIdAsync(int userId)
    {
        // DateTime.Now (saate duyarlı) değil DateTime.Today (gün başı) kullanıyoruz —
        // GoalStatus'un DateOnly bazlı son-7-gün penceresiyle aynı sınırda olsun diye.
        // Aksi halde günün ilerleyen saatlerinde bazı notlar bu listeden sessizce düşüyordu.
        var sevenDaysAgo = DateTime.Today.AddDays(-7);

        return await Context.StatusNotes
            .Where(sn => sn.UserId == userId && sn.CreatedAt >= sevenDaysAgo)
            .ToListAsync();
    }

    public async Task<List<StatusNote>> GetAllByUserIdAsync(int userId)
    {
        return await Context.StatusNotes
            .Where(sn => sn.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<StatusNote>> GetAllByUserIdIncludingDeletedAsync(int userId)
    {
        return await Context.StatusNotes
            .IgnoreQueryFilters()
            .Where(sn => sn.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<StatusNote>> GetByGoalStatusIdsAsync(IEnumerable<int> goalStatusIds)
    {
        return await Context.StatusNotes
            .Where(sn => sn.GoalStatusId != null && goalStatusIds.Contains(sn.GoalStatusId.Value))
            .ToListAsync();
    }
}
