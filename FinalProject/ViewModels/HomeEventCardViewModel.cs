namespace FinalProject.ViewModels;

// Картка події на головній сторінці.
public class HomeEventCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TitleDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
