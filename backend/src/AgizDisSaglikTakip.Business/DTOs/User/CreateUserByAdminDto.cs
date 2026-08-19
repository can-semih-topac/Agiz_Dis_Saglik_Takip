using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.DTOs.User;

public class CreateUserByAdminDto
{
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    // Role = Admin iken zorunlu. Role = User iken opsiyonel — boş bırakılırsa hesap şifresiz oluşturulup
    // davet e-postası gönderilir, doldurulursa admin ile aynı mantıkla geçici şifre atanır.
    public string? TemporaryPassword { get; set; }
}
