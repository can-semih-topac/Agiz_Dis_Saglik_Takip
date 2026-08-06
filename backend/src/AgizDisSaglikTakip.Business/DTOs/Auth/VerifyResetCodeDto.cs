namespace AgizDisSaglikTakip.Business.DTOs.Auth;

public class VerifyResetCodeDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
