using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface IStatusNoteRepository : IRepository<StatusNote>
{
    Task<List<StatusNote>> GetLast7DaysByUserIdAsync(int userId);
    Task<List<StatusNote>> GetByGoalStatusIdsAsync(IEnumerable<int> goalStatusIds);
}
