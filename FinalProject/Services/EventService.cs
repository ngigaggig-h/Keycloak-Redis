using System.Text.Json;
using FinalProject.Data;
using FinalProject.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FinalProject.Services;

// Сервіс доступу до подій з Redis кешуванням.
// Контролер Home не працює з DbContext напряму для читання подій,
// а викликає цей сервіс, щоб не змішувати UI-логіку і запити до БД.
// Кешує три основних запити: GetUpcomingEvents, GetEventById, GetCategoriesAsync.
public class EventService : IEventService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;

    
    private const string CategoriesKey = "all_categories";

    // Час життя кешу для різних запитів. Майбутні події змінюються частіше, ніж категорії.
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CategoriesCacheDuration = TimeSpan.FromHours(1);

    public EventService(ApplicationDbContext dbContext, IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int skip = 0, int take = 8, string? searching = null, IReadOnlyCollection<int>? categoryIds = null, string? sortBy = null, bool onlyUpcoming = false)
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
        // Кешується в Redis на 5 хвилин.
        var cacheKey = $"upcoming_events_{count}";

        try
        {
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedJson = System.Text.Encoding.UTF8.GetString(cachedData);
                var deserialized = JsonSerializer.Deserialize<List<EventDto>>(cachedJson);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
        }
        catch (Exception)
        {
            // Якщо десеріалізація не вдається, очищуємо кеш і отримуємо свіжі дані
            await _cache.RemoveAsync(cacheKey);
        }

        var result = await _dbContext.Events
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

        var json = JsonSerializer.Serialize(result);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        await _cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = DefaultCacheDuration
        });

        return result;
    }

    public async Task<EventDto?> GetEventById(int id)
    {
        // Одна подія для сторінки Details.
        // Кешується в Redis на 5 хвилин.
        var cacheKey = $"event_{id}";

        try
        {
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedJson = System.Text.Encoding.UTF8.GetString(cachedData);
                var deserialized = JsonSerializer.Deserialize<EventDto>(cachedJson);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
        }
        catch (Exception)
        {
            // Якщо десеріалізація не вдається, очищуємо кеш і отримуємо свіжі дані
            await _cache.RemoveAsync(cacheKey);
        }

        var result = await _dbContext.Events
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

        if (result != null)
        {
            var json = JsonSerializer.Serialize(result);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            await _cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = DefaultCacheDuration
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
    {
        // Отримання всіх категорій для меню фільтрів.
        // Кешується в Redis на 1 годину.
        var cacheKey = CategoriesKey;

        try
        {
            var cachedData = await _cache.GetAsync(cacheKey);
            if (cachedData != null)
            {
                var cachedJson = System.Text.Encoding.UTF8.GetString(cachedData);
                var deserialized = JsonSerializer.Deserialize<List<CategoryDto>>(cachedJson);
                if (deserialized != null)
                {
                    return deserialized;
                }
            }
        }
        catch (Exception)
        {
            // Якщо десеріалізація не вдається, очищуємо кеш і отримуємо свіжі дані
            await _cache.RemoveAsync(cacheKey);
        }

        var result = await _dbContext.Categories
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug
            })
            .ToListAsync();

        var json = JsonSerializer.Serialize(result);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        await _cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CategoriesCacheDuration
        });

        return result;
    }

    /// <summary>
    /// Інвалідує кеш подій. Викликається після створення, редагування або видалення події.
    /// Якщо передано eventId, інвалідується кеш конкретної події.
    /// Завжди інвалідується кеш майбутніх подій.
    /// </summary>
    public async Task InvalidateEventCacheAsync(int? eventId = null)
    {
        if (eventId.HasValue)
        {
            await _cache.RemoveAsync($"event_{eventId}");
        }

        // Завжди інвалідуємо кеш майбутніх подій, бо вони могли змінитися.
        for (int i = 1; i <= 10; i++)
        {
            await _cache.RemoveAsync($"upcoming_events_{i}");
        }
    }

    /// <summary>
    /// Інвалідує кеш категорій. Викликається після створення, редагування або видалення категорії.
    /// Завжди інвалідується кеш категорій, бо вони могли змінитися.
    /// </summary>
    public async Task InvalidateCategoriesCacheAsync()
    {
        await _cache.RemoveAsync(CategoriesKey);
    }
}
