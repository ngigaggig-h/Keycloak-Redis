namespace FinalProject.Models;

// Зв'язка "користувач зберіг подію".
public class SavedEvent
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
