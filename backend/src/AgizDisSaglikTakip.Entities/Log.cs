namespace AgizDisSaglikTakip.Entities;

public class Log
{
    public int Id { get; set; }
    // ILogger'ın LogLevel'ı (Warning, Error, Critical...) metin olarak.
    public string Level { get; set; } = string.Empty;
    // Logu üreten sınıf (ör. "AgizDisSaglikTakip.Business.Concrete.AuthManager").
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    // Bir exception ile loglandıysa stack trace dahil tam metni; yoksa null.
    public string? Exception { get; set; }
    public DateTime CreatedAt { get; set; }
}
