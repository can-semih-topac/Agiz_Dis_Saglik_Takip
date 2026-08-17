namespace AgizDisSaglikTakip.Business.DTOs.Log;

public class LogDto
{
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public DateTime CreatedAt { get; set; }
}
