namespace FinalProject.Models;

// Зв'язка "користувач забронював подію".
public class BookedEvent
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
