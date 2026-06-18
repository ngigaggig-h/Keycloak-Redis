using FinalProject.DTOs;

namespace FinalProject.Services;

// Контракт сервісу читання подій для контролера Home.
public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int skip = 0, int take = 8, string? searching = null, IReadOnlyCollection<int>? categoryIds = null, string? sortBy = null, bool onlyUpcoming = false);
    Task<int> GetEventsCount(string? searching = null, IReadOnlyCollection<int>? categoryIds = null, bool onlyUpcoming = false);
    Task<IReadOnlyList<EventDto>> GetUpcomingEvents(int count = 3);
    Task<EventDto?> GetEventById(int id);
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync();
}
