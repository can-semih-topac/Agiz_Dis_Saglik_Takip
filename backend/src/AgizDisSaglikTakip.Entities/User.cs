namespace AgizDisSaglikTakip.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordEncrypted { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<StatusNote> StatusNotes { get; set; } = new List<StatusNote>();
}
