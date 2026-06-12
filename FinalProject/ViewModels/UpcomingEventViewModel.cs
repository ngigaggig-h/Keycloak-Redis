namespace FinalProject.ViewModels;

// Модель елемента блоку "Найближчі події" на головній.
public class UpcomingEventViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
}
