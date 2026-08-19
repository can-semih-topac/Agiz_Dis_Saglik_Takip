using AgizDisSaglikTakip.Business.DTOs.StatusNote;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IStatusNoteService
{
    Task<ServiceResult> CreateStatusNoteAsync(int userId, CreateStatusNoteDto dto);
    Task<ServiceResult> UpdateStatusNoteAsync(int userId, int id, UpdateStatusNoteDto dto);
    Task<ServiceResult<List<StatusNoteDto>>> GetLast7DaysAsync(int userId);
    // Takvim görünümü — herhangi bir ayı gezebilmek için tüm geçmiş notlar gerekiyor.
    Task<ServiceResult<List<StatusNoteDto>>> GetAllAsync(int userId);
}
