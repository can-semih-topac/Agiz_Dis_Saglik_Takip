namespace AgizDisSaglikTakip.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    // Google ile oluşturulan hesaplarda kullanıcı henüz kendi şifresini belirlemediği için boş olabilir.
    public string? PasswordEncrypted { get; set; }
    public string FullName { get; set; } = string.Empty;
    // Google ile oluşturulan hesaplarda kullanıcı henüz belirtmediği için boş olabilir.
    public DateOnly? BirthDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Parola hatırlatma akışı için — kod üretilince dolar, kullanılınca/süresi geçince temizlenir.
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiresAt { get; set; }

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<StatusNote> StatusNotes { get; set; } = new List<StatusNote>();
}
