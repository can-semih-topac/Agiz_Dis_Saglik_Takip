namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class GoalStatusDto
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsApplied { get; set; }
}
