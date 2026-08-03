using AgizDisSaglikTakip.DataAccess.Abstract;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Concrete.EntityFramework;

public class EfSuggestionRepository : EfRepositoryBase<Suggestion>, ISuggestionRepository
{
    public EfSuggestionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Suggestion?> GetRandomAsync()
    {
        var count = await Context.Suggestions.CountAsync();
        if (count == 0)
            return null;

        var randomIndex = Random.Shared.Next(count);
        return await Context.Suggestions.Skip(randomIndex).FirstOrDefaultAsync();
    }
}
