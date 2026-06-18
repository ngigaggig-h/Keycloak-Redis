namespace FinalProject.ViewModels;

// Дані для сторінки деталей конкретної події.
public class EventDetailsViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool IsSaved { get; set; }
    public bool IsBooked { get; set; }
}
