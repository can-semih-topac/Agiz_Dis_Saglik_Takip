using AgizDisSaglikTakip.Entities;

namespace AgizDisSaglikTakip.DataAccess.Abstract;

public interface ISuggestionRepository : IRepository<Suggestion>
{
    Task<Suggestion?> GetRandomAsync();
}
