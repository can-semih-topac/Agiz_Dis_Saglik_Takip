namespace AgizDisSaglikTakip.Business.DTOs.AdminActionLog;

public class AdminActionLogDto
{
    public int Id { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TargetEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
