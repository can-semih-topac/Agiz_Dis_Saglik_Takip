namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class CreateGoalStatusDto
{
    public int GoalId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsApplied { get; set; }
}
