using AgizDisSaglikTakip.Business.DTOs.Suggestion;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface ISuggestionService
{
    Task<ServiceResult<SuggestionDto>> GetRandomAsync();
}
