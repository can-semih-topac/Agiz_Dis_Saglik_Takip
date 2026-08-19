namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class UpdateGoalStatusDto
{
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    // Hedefin TrackingType'ı Yapildi ise bu alan yok sayılır (null kaydedilir).
    public int? DurationMinutes { get; set; }
}
