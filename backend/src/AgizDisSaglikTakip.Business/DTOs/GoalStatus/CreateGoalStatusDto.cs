namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class CreateGoalStatusDto
{
    public int GoalId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    // Hedefin TrackingType'ı Yapildi ise bu alan yok sayılır (null kaydedilir).
    public int? DurationMinutes { get; set; }
}
