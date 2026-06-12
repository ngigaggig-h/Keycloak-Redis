using System.Security.Claims;
using FinalProject.Data;
using FinalProject.Models;
using FinalProject.Services;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace FinalProject.Controllers;

// Контролер акаунта.
// Відповідає за весь цикл користувача:
// реєстрація -> вхід -> робота в сесії -> перегляд профілю -> вихід.
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ApplicationDbContext _dbContext;

    public AccountController(IAuthService authService, ApplicationDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Повертає профіль поточного авторизованого користувача.
        // Додатково підтягує дві колекції:
        // - збережені події;
        // - заброньовані події.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var savedEvents = await _dbContext.SavedEvents
            .Include(savedEvent => savedEvent.Event)
            .Where(savedEvent => savedEvent.UserId == userId && savedEvent.Event != null)
            .OrderByDescending(savedEvent => savedEvent.SavedAt)
            .Select(savedEvent => new SavedEventProfileItemViewModel
            {
                EventId = savedEvent.EventId,
                Title = savedEvent.Event!.Title,
                Location = savedEvent.Event.Location,
                StartAt = savedEvent.Event.StartAt,
                SavedAt = savedEvent.SavedAt
            })
            .ToListAsync();

        var bookedEvents = await _dbContext.BookedEvents
            .Include(bookedEvent => bookedEvent.Event)
            .Where(bookedEvent => bookedEvent.UserId == userId && bookedEvent.Event != null)
            .OrderByDescending(bookedEvent => bookedEvent.BookedAt)
            .Select(bookedEvent => new BookedEventProfileItemViewModel
            {
                EventId = bookedEvent.EventId,
                Title = bookedEvent.Event!.Title,
                Location = bookedEvent.Event.Location,
                StartAt = bookedEvent.Event.StartAt,
                BookedAt = bookedEvent.BookedAt
            })
            .ToListAsync();

        var profileModel = new ProfileViewModel
        {
            Name = User.Identity!.Name!,
            Email = User.FindFirstValue(ClaimTypes.Email)!,
            Role = User.FindFirstValue(ClaimTypes.Role)!,
            SavedEvents = savedEvents,
            BookedEvents = bookedEvents
        };

        return View(profileModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "/"
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
            );
    }
}
