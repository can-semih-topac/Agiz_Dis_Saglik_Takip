namespace AgizDisSaglikTakip.Entities;

// Bir hedefin belirli bir süre (tatil, hastalık vb.) askıya alınması — bu süre boyunca
// seri hesaplamasında ne bozan ne de sayılan, "yok sayılan" bir aralık oluşturur.
public class GoalPause
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    // null = hâlâ duraklatılmış, devam ediyor.
    public DateOnly? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Goal Goal { get; set; } = null!;
}
