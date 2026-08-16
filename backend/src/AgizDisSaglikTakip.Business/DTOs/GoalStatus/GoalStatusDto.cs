using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.DTOs.GoalStatus;

public class GoalStatusDto
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public TrackingType TrackingType { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly ActivityTime { get; set; }
    public int? DurationMinutes { get; set; }
    // Bu kaydın tarihine kadar, bu hedef için kesintisiz kaç gündür kayıt girildiği.
    public int StreakCount { get; set; }
}
