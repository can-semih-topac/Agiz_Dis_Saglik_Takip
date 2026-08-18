namespace AgizDisSaglikTakip.Entities;

public class StatusNote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    // Durum kaydı formuyla birlikte oluşturulduysa o kayda bağlanır (opsiyonel).
    public int? GoalStatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    // Yumuşak silme — bkz. User.IsDeleted.
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
    public GoalStatus? GoalStatus { get; set; }
}
