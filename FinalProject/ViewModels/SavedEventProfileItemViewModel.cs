namespace FinalProject.ViewModels;

// Один елемент списку збережених подій у профілі.
public class SavedEventProfileItemViewModel
{
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime SavedAt { get; set; }
}
