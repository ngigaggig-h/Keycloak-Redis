namespace FinalProject.ViewModels;

// Модель часткового шаблону EventsChunk для підвантаження списку подій.
public class EventsChunkViewModel
{
    public List<HomeEventCardViewModel> Events { get; set; } = new();
    public int NextSkip { get; set; }
    public bool HasMore { get; set; }
}
