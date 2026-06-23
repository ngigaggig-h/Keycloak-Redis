using FinalProject.Data;
using FinalProject.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services;

// Сервіс доступу до подій.
// Контролер Home не працює з DbContext напряму для читання подій,
// а викликає цей сервіс, щоб не змішувати UI-логіку і запити до БД.
public class EventService : IEventService
{
    private readonly ApplicationDbContext _dbContext;

    public EventService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int skip = 0, int take = 10, string? searching = null, IReadOnlyCollection<int>? categoryIds = null, string? sortBy = null, bool onlyUpcoming = false)
    {
        // Базовий запит списку подій для головної.
        // Далі до нього додаємо пошук, пагінацію і маппінг у DTO.
        var query = _dbContext.Events
            .Include(eventEntity => eventEntity.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searching))
        {
            var normalizedSearching = searching.Trim().ToLower();
            query = query.Where(eventEntity => eventEntity.Title.ToLower().Contains(normalizedSearching));
        }

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(eventEntity => categoryIds.Contains(eventEntity.CategoryId));
        }

        if (onlyUpcoming)
        {
            query = query.Where(eventEntity => eventEntity.StartAt > DateTime.UtcNow);
        }

        query = sortBy?.ToLower() switch
        {
            "title" => query.OrderBy(eventEntity => eventEntity.Title),
            "capacity" => query.OrderByDescending(eventEntity => eventEntity.Capacity),
            _ => query.OrderBy(eventEntity => eventEntity.StartAt)
        };

        return await query
            .Skip(skip)
            .Take(take)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                TitleDescription = e.TitleDescription,
                Location = e.Location,
                StartAt = e.StartAt,
                Capacity = e.Capacity,
                ImageUrl = e.ImageUrl,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty
            })
            .ToListAsync();
    }

    public async Task<int> GetEventsCount(string? searching = null, IReadOnlyCollection<int>? categoryIds = null, bool onlyUpcoming = false)
    {
        // Кількість потрібна для розрахунку пагінації.
        var query = _dbContext.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searching))
        {
            var normalizedSearching = searching.Trim().ToLower();
            query = query.Where(eventEntity => eventEntity.Title.ToLower().Contains(normalizedSearching));
        }

        if (categoryIds is { Count: > 0 })
        {
            query = query.Where(eventEntity => categoryIds.Contains(eventEntity.CategoryId));
        }

        if (onlyUpcoming)
        {
            query = query.Where(eventEntity => eventEntity.StartAt > DateTime.UtcNow);
        }

        return await query.CountAsync();
    }

    public async Task<IReadOnlyList<EventDto>> GetUpcomingEvents(int count = 3)
    {
        // Короткий список найближчих подій (бічна колонка на головній).
        return await _dbContext.Events
            .OrderBy(e => e.StartAt)
            .Take(count)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                TitleDescription = e.TitleDescription,
                Location = e.Location,
                StartAt = e.StartAt,
                Capacity = e.Capacity,
                ImageUrl = e.ImageUrl,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty
            })
            .ToListAsync();
    }

    public async Task<EventDto?> GetEventById(int id)
    {
        // Одна подія для сторінки Details.
        return await _dbContext.Events
            .Where(e => e.Id == id)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                TitleDescription = e.TitleDescription,
                Location = e.Location,
                StartAt = e.StartAt,
                Capacity = e.Capacity,
                ImageUrl = e.ImageUrl,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            })
            .ToListAsync();
    }
}
