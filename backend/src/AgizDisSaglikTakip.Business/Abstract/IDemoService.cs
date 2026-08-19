using AgizDisSaglikTakip.Business.DTOs.Auth;
using AgizDisSaglikTakip.Core.Utilities.Results;

namespace AgizDisSaglikTakip.Business.Abstract;

public interface IDemoService
{
    // Demo hesabının verilerini şablondan (tarihleri güncel görünecek şekilde kaydırarak) yeniden
    // oluşturup, hazır hale gelen bu hesaba giriş token'ı döner.
    Task<ServiceResult<LoginResultDto>> EnterDemoAsync();
}
