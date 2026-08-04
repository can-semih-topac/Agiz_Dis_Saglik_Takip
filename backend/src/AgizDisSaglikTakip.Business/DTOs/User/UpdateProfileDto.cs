namespace AgizDisSaglikTakip.Business.DTOs.User;

public class UpdateProfileDto
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? NewPassword { get; set; } // Boş bırakılırsa parola değiştirilmez.
    public string? NewPasswordConfirm { get; set; }
}
