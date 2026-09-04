namespace AgizDisSaglikTakip.Business.DTOs.Auth;

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
