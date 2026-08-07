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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordEncrypted).IsRequired(false);
            entity.Property(u => u.FullName).HasMaxLength(150).IsRequired();
            // Mevcut kayıtlarda (migration öncesi) bu alan yoktu, boş metin varsayılanıyla dolduruluyor.
            entity.Property(u => u.PhoneNumber).HasMaxLength(15).IsRequired().HasDefaultValue(string.Empty);
            entity.Property(u => u.PasswordResetCode).HasMaxLength(6);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.Property(g => g.Title).HasMaxLength(150).IsRequired();
            entity.Property(g => g.Description).HasMaxLength(500).IsRequired();

            entity.HasOne(g => g.User)
                  .WithMany(u => u.Goals)
                  .HasForeignKey(g => g.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GoalStatus>(entity =>
        {
            entity.HasOne(gs => gs.Goal)
                  .WithMany(g => g.GoalStatuses)
                  .HasForeignKey(gs => gs.GoalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StatusNote>(entity =>
        {
            entity.Property(sn => sn.Description).HasMaxLength(1000).IsRequired();
            entity.Property(sn => sn.ImagePath).HasMaxLength(500);

            entity.HasOne(sn => sn.User)
                  .WithMany(u => u.StatusNotes)
                  .HasForeignKey(sn => sn.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
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
