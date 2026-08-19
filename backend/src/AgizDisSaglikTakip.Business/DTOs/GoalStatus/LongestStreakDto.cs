namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class LongestStreakDto
{
    public int GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public int LongestStreak { get; set; }
}
