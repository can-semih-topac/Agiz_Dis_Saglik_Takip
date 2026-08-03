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
        var sevenDaysAgo = DateTime.Now.AddDays(-7);

        return await Context.StatusNotes
            .Where(sn => sn.UserId == userId && sn.CreatedAt >= sevenDaysAgo)
            .ToListAsync();
    }
}
