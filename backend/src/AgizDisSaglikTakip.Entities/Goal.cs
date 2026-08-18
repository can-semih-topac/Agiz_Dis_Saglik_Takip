using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Entities;

public class Goal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PeriodUnit PeriodUnit { get; set; }
    public int PeriodFrequency { get; set; }
    public Importance Importance { get; set; }
    // Bu hedefe ait durum kayıtları süre girilerek mi yoksa sadece "yapıldı" işaretiyle mi tutulacak.
    public TrackingType TrackingType { get; set; }
    public DateTime CreatedAt { get; set; }
    // Yumuşak silme — bkz. User.IsDeleted.
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
    public ICollection<GoalStatus> GoalStatuses { get; set; } = new List<GoalStatus>();
}
