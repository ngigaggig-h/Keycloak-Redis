namespace FinalProject.ViewModels;

// Загальна модель профілю користувача.
public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<SavedEventProfileItemViewModel> SavedEvents { get; set; } = new();
    public List<BookedEventProfileItemViewModel> BookedEvents { get; set; } = new();
}
