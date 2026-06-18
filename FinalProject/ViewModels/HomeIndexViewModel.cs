namespace FinalProject.ViewModels;

// Дані для головної сторінки: список подій, пошук, пагінація, sidebar.
public class HomeIndexViewModel
{
    public List<HomeEventCardViewModel> Events { get; set; } = new();
    public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();
    public string SearchQuery { get; set; } = string.Empty;
    public List<int> SelectedCategoryIds { get; set; } = new();
    public string SortBy { get; set; } = "date";
    public bool OnlyUpcoming { get; set; }
    public List<CategoryFilterOptionViewModel> Categories { get; set; } = new();
    public int Skip { get; set; }
    public int Take { get; set; }
    public int TotalCount { get; set; }
    public int NextSkip => Skip + Take;
    public bool HasMore => NextSkip < TotalCount;
}
