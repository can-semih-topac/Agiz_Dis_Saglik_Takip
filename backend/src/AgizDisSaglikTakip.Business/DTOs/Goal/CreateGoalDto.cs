using AgizDisSaglikTakip.Entities.Enums;

namespace AgizDisSaglikTakip.Business.DTOs.Goal;

public class CreateGoalDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PeriodUnit PeriodUnit { get; set; }
    public int PeriodFrequency { get; set; }
    public Importance Importance { get; set; }
}
