using AgizDisSaglikTakip.Business.DTOs.Contact;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IContactService
{
    Task<ServiceResult> SendMessageAsync(SendContactMessageDto dto);
}
