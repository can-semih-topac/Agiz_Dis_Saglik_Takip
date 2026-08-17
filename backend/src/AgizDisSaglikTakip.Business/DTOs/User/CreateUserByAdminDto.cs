using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.DTOs.User;

public class CreateUserByAdminDto
{
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    // Sadece Role = Admin iken zorunlu; Role = User iken kullanılmaz (hesap şifresiz, davetle oluşturulur).
    public string? TemporaryPassword { get; set; }
}
