using FinalProject.DTOs;

namespace FinalProject.Services.Interfaces;

// Контракт сервісу читання подій для контролера Home.
public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int skip = 0, int take = 8, string? searching = null);
    Task<int> GetEventsCount(string? searching = null);
    Task<IReadOnlyList<EventDto>> GetUpcomingEvents(int count = 3);
    Task<EventDto?> GetEventById(int id);
    Task InvalidateCacheAsync(int? eventId = null);
}
