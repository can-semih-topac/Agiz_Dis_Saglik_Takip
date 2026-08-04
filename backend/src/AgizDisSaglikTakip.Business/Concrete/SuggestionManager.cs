using AgizDisSaglikTakip.Business.Abstract;
using AgizDisSaglikTakip.Business.DTOs.Suggestion;
using AgizDisSaglikTakip.Core.Utilities.Results;
using AgizDisSaglikTakip.DataAccess.Abstract;

namespace AgizDisSaglikTakip.Business.Concrete;

public class SuggestionManager : ISuggestionService
{
    private readonly ISuggestionRepository _suggestionRepository;

    public SuggestionManager(ISuggestionRepository suggestionRepository)
    {
        _suggestionRepository = suggestionRepository;
    }

    public async Task<ServiceResult<SuggestionDto>> GetRandomAsync()
    {
        var suggestion = await _suggestionRepository.GetRandomAsync();
        if (suggestion == null)
            return ServiceResult<SuggestionDto>.Fail("Öneri bulunamadı.");

        return ServiceResult<SuggestionDto>.Ok(new SuggestionDto { Text = suggestion.Text });
    }
}
