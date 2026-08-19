namespace AgizDisSaglikTakip.Entities;

// İşlemi yapan admin'e kasıtlı olarak FK değil — admin hesabı ileride silinse bile
// bu geçmiş kaydı (kim, ne zaman, ne yaptı) kalıcı olarak kalmalı.
public class AdminActionLog
{
    public int Id { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
