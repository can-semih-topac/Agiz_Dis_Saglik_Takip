using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.User;
    // Google ile oluşturulan hesaplarda kullanıcı henüz kendi şifresini belirlemediği için boş olabilir.
    public string? PasswordEncrypted { get; set; }
    // Admin panelinden geçici şifreyle oluşturulan admin hesaplarında true olur, şifre değiştirilince false'a döner.
    public bool MustChangePassword { get; set; }
    // "Tanıtımı Göster" ile giriş yapılan tek canlı demo hesabında true — her girişte verileri sıfırlanır.
    public bool IsDemo { get; set; }
    // Yumuşak silme — hiçbir kayıt veritabanından fiziksel olarak silinmiyor, bu alan true olunca
    // global query filter sayesinde uygulamanın geri kalanına görünmez olur.
    public bool IsDeleted { get; set; }
    public string FullName { get; set; } = string.Empty;
    // Google ile oluşturulan hesaplarda kullanıcı henüz belirtmediği için boş olabilir.
    public DateOnly? BirthDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Şifre hatırlatma akışı için — kod üretilince dolar, kullanılınca/süresi geçince temizlenir.
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiresAt { get; set; }

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<StatusNote> StatusNotes { get; set; } = new List<StatusNote>();
}
