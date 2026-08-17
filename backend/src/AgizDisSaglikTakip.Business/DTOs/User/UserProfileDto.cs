namespace AgizDisSaglikTakip.Business.DTOs.User;

public class UserProfileDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    // Google ile oluşturulup henüz şifre belirlememiş hesaplarda false olur.
    public bool HasPassword { get; set; }
    // Admin panelinden geçici şifreyle oluşturulan hesaplarda, şifre değiştirilene kadar true olur.
    public bool MustChangePassword { get; set; }
}
