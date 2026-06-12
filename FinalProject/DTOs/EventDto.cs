namespace FinalProject.DTOs;

// DTO для передачі даних події із сервісу в контролер.
// Дозволяє не віддавати EF-сутність напряму у UI.
public class EventDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TitleDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public int Capacity { get; set; }
    public string? ImageUrl { get; set; }
}
