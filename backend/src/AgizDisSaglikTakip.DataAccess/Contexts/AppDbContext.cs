using AgizDisSaglikTakip.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgizDisSaglikTakip.DataAccess.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalStatus> GoalStatuses => Set<GoalStatus>();
    public DbSet<StatusNote> StatusNotes => Set<StatusNote>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Log> Logs => Set<Log>();
    public DbSet<AdminActionLog> AdminActionLogs => Set<AdminActionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            // Filtreli: sadece silinmemiş kayıtlar arasında tekil — yumuşak silinen bir hesabın
            // e-postası aksi halde bir daha hiç kayıt/davet için kullanılamazdı.
            entity.HasIndex(u => u.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.Property(u => u.PasswordEncrypted).IsRequired(false);
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            // Mevcut kayıtlarda (migration öncesi) bu alan yoktu, boş metin varsayılanıyla dolduruluyor.
            entity.Property(u => u.PhoneNumber).HasMaxLength(15).IsRequired().HasDefaultValue(string.Empty);
            entity.Property(u => u.PasswordResetCode).HasMaxLength(6);
            entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.Property(g => g.Title).HasMaxLength(150).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(500).IsRequired();
            entity.Property(g => g.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(g => !g.IsDeleted);

            entity.HasOne(g => g.User)
                  .WithMany(u => u.Goals)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GoalStatus>(entity =>
        {
            entity.Property(gs => gs.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(gs => !gs.IsDeleted);

            entity.HasOne(gs => gs.Goal)
                  .WithMany(g => g.GoalStatuses)
                  .HasForeignKey(gs => gs.GoalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusNote>(entity =>
        {
            entity.Property(sn => sn.Description).HasMaxLength(1000).IsRequired();
            entity.Property(sn => sn.ImagePath).HasMaxLength(500);
            entity.Property(sn => sn.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(sn => !sn.IsDeleted);

            entity.HasOne(sn => sn.User)
                  .WithMany(u => u.StatusNotes)
                  .HasForeignKey(sn => sn.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Restrict (NO ACTION): User->StatusNote zaten cascade olduğu için buraya Cascade/SetNull
            // eklemek SQL Server'da "multiple cascade paths" hatası veriyor (User->Goal->GoalStatus->StatusNote).
            // Bağlantı, GoalManager.DeleteGoalAsync içinde hedef silinmeden önce elle koparılıyor.
            entity.HasOne(sn => sn.GoalStatus)
                  .WithMany()
                  .HasForeignKey(sn => sn.GoalStatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.Property(c => c.FullName).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(256).IsRequired();
            entity.Property(c => c.Message).HasMaxLength(2000).IsRequired();
            entity.Property(c => c.ImagePath).HasMaxLength(500);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.Property(l => l.Level).HasMaxLength(20).IsRequired();
            entity.Property(l => l.Category).HasMaxLength(300).IsRequired();
            entity.Property(l => l.Message).IsRequired();
        });

        modelBuilder.Entity<AdminActionLog>(entity =>
        {
            entity.Property(a => a.AdminEmail).HasMaxLength(256).IsRequired();
            entity.Property(a => a.Action).HasMaxLength(200).IsRequired();
            entity.Property(a => a.TargetEmail).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<Suggestion>(entity =>
        {
            entity.Property(s => s.Text).HasMaxLength(500).IsRequired();

            entity.HasData(
                new Suggestion { Id = 1, Text = "Dişlerinizi günde en az iki kez, sabah ve akşam fırçalayın." },
                new Suggestion { Id = 2, Text = "Diş ipini her gün kullanarak diş aralarındaki plağı temizleyin." },
                new Suggestion { Id = 3, Text = "Şekerli ve asitli içecekleri azaltarak dişlerinizi çürükten koruyun." },
                new Suggestion { Id = 4, Text = "Diş fırçanızı her 3 ayda bir, kılları yıprandığında yenileyin." },
                new Suggestion { Id = 5, Text = "Ağız gargarası kullanarak ağız kokusunu ve bakteri oluşumunu azaltabilirsiniz." },
                new Suggestion { Id = 6, Text = "Diş hekiminizi yılda en az iki kez düzenli kontrol için ziyaret edin." },
                new Suggestion { Id = 7, Text = "Sert kıllı yerine yumuşak kıllı diş fırçası tercih edin." },
                new Suggestion { Id = 8, Text = "Asitli gıdalardan sonra dişlerinizi hemen değil, 30 dakika bekleyip fırçalayın." },
                new Suggestion { Id = 9, Text = "Bol su içerek ağzınızın kurumasını önleyin, tükürük diş sağlığını korur." },
                new Suggestion { Id = 10, Text = "Tırnak yeme ve kalem çiğneme gibi alışkanlıklardan kaçının, dişlerinize zarar verir." }
            );
        });
    }
}
