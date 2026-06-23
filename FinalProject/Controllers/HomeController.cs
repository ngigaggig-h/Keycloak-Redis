using System.Security.Claims;
using FinalProject.Data;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers;

// Публічний контролер для роботи з подіями.
// Цей контролер обслуговує основний user-flow:
// - перегляд списку подій;
// - перегляд деталей;
// - збереження події;
// - бронювання місця;
// - видалення події (для ролей Admin/Organizer).
public class HomeController : Controller
{
    private const int PageSize = 10;

    private readonly IEventService _eventService;
    private readonly ApplicationDbContext _dbContext;

    public HomeController(IEventService eventService, ApplicationDbContext dbContext)
    {
        _eventService = eventService;
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(int skip = 0, int take = PageSize, string? searching = null, int[]? categoryIds = null, string? sortBy = null, bool onlyUpcoming = false)
    {
        // Головна сторінка з пагінацією і пошуком.
        // skip/take приходять із query string.
        if (skip < 0) skip = 0;
        take = PageSize;

        var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "date" : sortBy;
        var selectedCategoryIds = categoryIds?.Distinct().ToArray() ?? [];
        var eventsFromDb = await _eventService.GetLatestEventsAsync(skip, take, searching, selectedCategoryIds, normalizedSortBy, onlyUpcoming);
        var upcomingEvents = await _eventService.GetUpcomingEvents(3);
        var totalEventsCount = await _eventService.GetEventsCount(searching, selectedCategoryIds, onlyUpcoming);
        var categories = await _eventService.GetCategoriesAsync();

        var pageModel = new HomeIndexViewModel
        {
            Events = eventsFromDb.Select(ToCard).ToList(),
            UpcomingEvents = upcomingEvents.Select(ToUpcoming).ToList(),
            SearchQuery = searching ?? string.Empty,
            SelectedCategoryIds = selectedCategoryIds.ToList(),
            SortBy = normalizedSortBy,
            OnlyUpcoming = onlyUpcoming,
            Categories = categories.Select(category => new CategoryFilterOptionViewModel
            {
                Id = category.Id,
                Name = category.Name
            }).ToList(),
            Skip = skip,
            Take = take,
            TotalCount = totalEventsCount
        };

        return View(pageModel);
    }

    [HttpGet]
    public async Task<IActionResult> LoadMore(int skip = 0, int take = PageSize, string? searching = null, int[]? categoryIds = null, string? sortBy = null, bool onlyUpcoming = false)
    {
        // Дія для кнопки "Завантажити ще".
        // Повертає не повну сторінку, а partial view з наступною порцією карток.
        if (skip < 0) 
            skip = 0;

        take = PageSize;

        var normalizedSortBy = string.IsNullOrWhiteSpace(sortBy) ? "date" : sortBy;
        var selectedCategoryIds = categoryIds?.Distinct().ToArray() ?? [];
        var eventsFromDb = await _eventService.GetLatestEventsAsync(skip, take, searching, selectedCategoryIds, normalizedSortBy, onlyUpcoming);
        var totalEventsCount = await _eventService.GetEventsCount(searching, selectedCategoryIds, onlyUpcoming);

        var chunkModel = new EventsChunkViewModel
        {
            Events = eventsFromDb.Select(ToCard).ToList(),
            NextSkip = skip + take,
            HasMore = (skip + take) < totalEventsCount
        };

        return PartialView("~/Views/Shared/EventsChunk.cshtml", chunkModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // Деталі конкретної події.
        // Також перевіряємо для поточного користувача:
        // чи подія збережена і чи вже заброньована.
        var eventData = await _eventService.GetEventById(id);
        if (eventData is null)
        {
            return NotFound();
        }

        var isSaved = false;
        var isBooked = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            isSaved = await _dbContext.SavedEvents
                .AnyAsync(savedEvent => savedEvent.UserId == userId && savedEvent.EventId == id);
            isBooked = await _dbContext.BookedEvents
                .AnyAsync(bookedEvent => bookedEvent.UserId == userId && bookedEvent.EventId == id);
        }

        var detailsModel = new EventDetailsViewModel
        {
            Id = eventData.Id,
            Title = eventData.Title,
            Description = eventData.Description,
            Location = eventData.Location,
            StartAt = eventData.StartAt,
            Capacity = eventData.Capacity,
            ImageUrl = eventData.ImageUrl,
            CategoryName = eventData.CategoryName,
            IsSaved = isSaved,
            IsBooked = isBooked
        };

        return View(detailsModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleSave(int id)
    {
        // Toggle-логіка:
        // якщо запису в SavedEvents нема -> створюємо;
        // якщо запис є -> видаляємо.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var savedEvent = await _dbContext.SavedEvents
            .FirstOrDefaultAsync(currentSavedEvent => currentSavedEvent.UserId == userId && currentSavedEvent.EventId == id);

        var isSaved = savedEvent is null;
        var message = isSaved ? "Подію збережено" : "Подію прибрано зі збережених";

        if (isSaved)
        {
            _dbContext.SavedEvents.Add(new SavedEvent
            {
                UserId = userId,
                EventId = id,
                SavedAt = DateTime.UtcNow
            });
        }
        else
        {
            _dbContext.SavedEvents.Remove(savedEvent!);
        }

        await _dbContext.SaveChangesAsync();

        if (IsAjaxRequest())
        {
            return Json(new
            {
                isSaved,
                buttonText = isSaved ? "Видалити зі збереженого" : "Зберегти",
                message
            });
        }

        TempData["ToastMessage"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBooking(int id)
    {
        // Toggle-логіка бронювання:
        // - якщо броні нема і є місце -> бронюємо;
        // - якщо бронь є -> скасовуємо.
        // Паралельно оновлюємо Capacity у Event.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var bookedEvent = await _dbContext.BookedEvents
            .FirstOrDefaultAsync(currentBookedEvent => currentBookedEvent.UserId == userId && currentBookedEvent.EventId == id);

        var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(currentEvent => currentEvent.Id == id);

        if (eventEntity == null)
        {
            return NotFound();
        }

        var isBooked = bookedEvent == null;
        var message = string.Empty;

        if (isBooked)
        {
            if (eventEntity.Capacity > 0 && eventEntity.StartAt > DateTime.UtcNow)
            {
                _dbContext.BookedEvents.Add(new BookedEvent
                {
                    UserId = userId,
                    EventId = id,
                    BookedAt = DateTime.UtcNow
                });
                eventEntity.Capacity -= 1;
                message = "Місце успішно заброньовано";
            }
            else
            {
                isBooked = false;
                message = "На цю подію вже не можна забронювати місце";
            }
        }
        else
        {
            _dbContext.BookedEvents.Remove(bookedEvent!);
            eventEntity.Capacity += 1;
            message = "Бронювання скасовано";
        }

        await _dbContext.SaveChangesAsync();

        if (IsAjaxRequest())
        {
            return Json(new
            {
                isBooked,
                capacity = eventEntity.Capacity,
                canBook = isBooked || (eventEntity.Capacity > 0 && eventEntity.StartAt > DateTime.UtcNow),
                buttonText = isBooked ? "Скасувати бронювання" : "Забронювати місце",
                message
            });
        }

        TempData["ToastMessage"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Organizer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        // Видалення події дозволено тільки ролям Admin/Organizer.
        // Перед видаленням самої події чистимо пов'язані записи
        // з таблиць SavedEvents та BookedEvents.
        var saved = _dbContext.SavedEvents.Where(x => x.EventId == id);
        var booked = _dbContext.BookedEvents.Where(x => x.EventId == id);
        _dbContext.SavedEvents.RemoveRange(saved);
        _dbContext.BookedEvents.RemoveRange(booked);

        var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(x => x.Id == id);
        if (eventEntity != null)
        {
            _dbContext.Events.Remove(eventEntity);
            await _dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private static HomeEventCardViewModel ToCard(EventDto eventData)
    {
        return new HomeEventCardViewModel
        {
            Id = eventData.Id,
            Title = eventData.Title,
            TitleDescription = eventData.TitleDescription,
            Description = eventData.Description,
            Location = eventData.Location,
            StartAt = eventData.StartAt,
            Capacity = eventData.Capacity,
            ImageUrl = eventData.ImageUrl,
            CategoryName = eventData.CategoryName
        };
    }

    private static UpcomingEventViewModel ToUpcoming(EventDto eventData)
    {
        return new UpcomingEventViewModel
        {
            Id = eventData.Id,
            Title = eventData.Title,
            Location = eventData.Location,
            StartAt = eventData.StartAt
        };
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers.XRequestedWith == "XMLHttpRequest"
            || Request.Headers.Accept.Any(value => value?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
    }
}
