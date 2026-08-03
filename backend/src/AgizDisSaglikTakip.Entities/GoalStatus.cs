namespace AgizDisSaglikTakip.Entities;

public class GoalStatus
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsApplied { get; set; }
    public DateTime CreatedAt { get; set; }

    public Goal Goal { get; set; } = null!;
}
