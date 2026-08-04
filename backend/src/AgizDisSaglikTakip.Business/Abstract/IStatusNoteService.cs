using AgizDisSaglikTakip.Business.DTOs.StatusNote;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IStatusNoteService
{
    Task<ServiceResult> CreateStatusNoteAsync(int userId, CreateStatusNoteDto dto);
    Task<ServiceResult<List<StatusNoteDto>>> GetLast7DaysAsync(int userId);
}
