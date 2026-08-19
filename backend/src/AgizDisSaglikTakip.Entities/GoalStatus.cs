namespace AgizDisSaglikTakip.Entities;

public class GoalStatus
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    // Hedefin TrackingType'ı Sureli ise dolu, Yapildi ise null.
    public int? DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    // Yumuşak silme — bkz. User.IsDeleted.
    public bool IsDeleted { get; set; }

    public Goal Goal { get; set; } = null!;
}
