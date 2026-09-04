using AgizDisSaglikTakip.Business.Constants;
using AgizDisSaglikTakip.Core.Utilities.Security.Hashing;
using AgizDisSaglikTakip.DataAccess.Contexts;
using AgizDisSaglikTakip.Entities;
using AgizDisSaglikTakip.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.Business.Seed;

// Sıfırdan (boş) bir veritabanında demo şablon hesabı hiç yoksa oluşturur — böylece taze bir
// makinede "docker compose up" ile kaldırılan proje de demo butonuyla gerçekten çalışır.
// Migration'lar gibi idempotent: şablon zaten varsa (mesela gerçek üretim verisi geri
// yüklendiğinde olduğu gibi) hiçbir şey yapmadan çıkar.
public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync(u => u.Email == DemoAccountConstants.TemplateEmail))
            return;

        var now = DateTime.Now;
        // Kimsenin bilmediği rastgele bir şifre — demo hesabına giriş zaten şifreyle değil,
        // "Tanıtımı Göster" butonuyla yapılıyor (bkz. DemoManager).
        var placeholderPassword = passwordHasher.Hash(Guid.NewGuid().ToString("N"));

        var template = new User
        {
            Email = DemoAccountConstants.TemplateEmail,
            FullName = "Canan Dinçel",
            PasswordHash = placeholderPassword,
            PhoneNumber = "5000000000",
            BirthDate = new DateOnly(1995, 4, 12),
            CreatedAt = now
        };
        var demo = new User
        {
            Email = DemoAccountConstants.DemoEmail,
            FullName = "Canan Dinçel",
            PasswordHash = placeholderPassword,
            IsDemo = true,
            PhoneNumber = "5000000000",
            CreatedAt = now
        };
        context.Users.AddRange(template, demo);
        await context.SaveChangesAsync();

        var goalDefs = new (string Title, PeriodUnit Unit, int Freq, TrackingType Tracking, Importance Importance)[]
        {
            ("Diş ipi", PeriodUnit.Gun, 1, TrackingType.Yapildi, Importance.Yuksek),
            ("Diş Fırçalama", PeriodUnit.Gun, 2, TrackingType.Sureli, Importance.Yuksek),
            ("Gargara Kulllanma", PeriodUnit.Gun, 3, TrackingType.Yapildi, Importance.Orta),
            ("Dilin Temizlenmesi", PeriodUnit.Gun, 1, TrackingType.Yapildi, Importance.Orta),
            ("Şekerli/Asitli İçecek Tüketimini Sınırlama", PeriodUnit.Gun, 1, TrackingType.Yapildi, Importance.Orta),
            ("Diş fırçası temizliği", PeriodUnit.Gun, 1, TrackingType.Sureli, Importance.Dusuk),
            ("Diş hekimi Muanesi", PeriodUnit.Ay, 6, TrackingType.Sureli, Importance.Yuksek),
            ("Diş Fırçası Yenileme Kontrolü", PeriodUnit.Ay, 1, TrackingType.Yapildi, Importance.Dusuk)
        };

        var goals = goalDefs.Select(d => new Goal
        {
            UserId = template.Id,
            Title = d.Title,
            PeriodUnit = d.Unit,
            PeriodFrequency = d.Freq,
            TrackingType = d.Tracking,
            Importance = d.Importance,
            CreatedAt = now.AddDays(-10)
        }).ToList();
        context.Goals.AddRange(goals);
        await context.SaveChangesAsync();

        // Son 3 gün için günlük hedeflere geçmiş kayıt ekleniyor — DemoManager zaten en son
        // günü "dün"e kaydırdığı için ziyaretçi demo hesabına her girdiğinde güncel görünür.
        var dailyTimes = new[] { new TimeOnly(8, 0), new TimeOnly(14, 0), new TimeOnly(20, 30) };
        var statuses = new List<GoalStatus>();
        foreach (var goal in goals.Where(g => g.PeriodUnit == PeriodUnit.Gun))
        {
            for (var dayOffset = 3; dayOffset >= 1; dayOffset--)
            {
                for (var i = 0; i < goal.PeriodFrequency; i++)
                {
                    statuses.Add(new GoalStatus
                    {
                        GoalId = goal.Id,
                        ActivityDate = DateOnly.FromDateTime(now.AddDays(-dayOffset)),
                        ActivityTime = dailyTimes[i % dailyTimes.Length],
                        DurationMinutes = goal.TrackingType == TrackingType.Sureli ? 2 : null,
                        CreatedAt = now.AddDays(-dayOffset)
                    });
                }
            }
        }
        context.GoalStatuses.AddRange(statuses);
        await context.SaveChangesAsync();
    }
}
