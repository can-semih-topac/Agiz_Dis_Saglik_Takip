using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.DTOs.User;

// Admin panelindeki kullanıcı listesi için — şifre/sıfırlama kodu gibi hassas alanlar bilerek yok.
public class UserAdminDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public Role Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public int WillpowerScore { get; set; }
}
