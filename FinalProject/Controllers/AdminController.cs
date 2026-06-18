using System.Security.Claims;
using FinalProject.Data;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers;

// Контролер адмін-панелі.
// Цей контролер відповідає за просте адміністрування у межах курсового:
// 1) створення події;
// 2) перегляд списку користувачів та подій;
// 3) редагування та видалення подій (CRUD);
// 4) видача ролі Organizer.
// Весь контролер закритий атрибутом [Authorize(Roles = "Admin")],
// тобто будь-яка дія всередині доступна тільки адміну.
[Authorize(Roles = "admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public AdminController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // [HttpGet("")] означає:
        // - HTTP метод: GET;
        // - порожній підмаршрут відносно [Route("admin")].
        // Підсумковий URL цієї дії: /admin
        var model = new AdminPageViewModel();
        await FillUsers(model);

        // Підтягуємо список подій з бази даних для таблиці керування (CRUD)
        model.Events = await _dbContext.Events
            .OrderByDescending(e => e.Id)
            .ToListAsync();

        return View(model);
    }

    // [HttpPost("add-event")] означає:
    // - HTTP метод: POST;
    // - підмаршрут "add-event" відносно [Route("admin")].
    // Підсумковий URL цієї дії: POST /admin/add-event
    // Саме цю адресу викликає форма створення події в адмінці.
    [HttpPost("add-event")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(AdminPageViewModel model)
    {
        // 1. Отримуємо унікальний ID користувача з токена Keycloak
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Користувач не авторизований через Keycloak.");
        }

        // 2. 🟢 ПЕРЕВІРКА ТА ЗАПИС У ТАБЛИЦЮ USERS
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            // Отримуємо додаткові дані з токена (email, ім'я)
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "no-email@keycloak.local";
            var userName = User.FindFirst("preferred_username")?.Value ?? "KeycloakUser";

            var newLocalUser = new User // Замініть на назву вашого класу користувача
            {
                Id = userId, // Передаємо саме той GUID-рядок з Keycloak
                Email = userEmail,
                Name = userName
            };

            _dbContext.Users.Add(newLocalUser);
            await _dbContext.SaveChangesAsync(); // Спочатку записуємо користувача в dbo.Users
        }

        // 3. Тепер створюємо подію — SQL Server більше не буде сваритися
        var newEvent = new Event
        {
            Title = model.Title,
            Description = model.Description,
            OrganizerId = userId // Тепер цей ID точно існує в базі!
        };

        _dbContext.Events.Add(newEvent);
        await _dbContext.SaveChangesAsync(); // Зберігаємо подію

        return RedirectToAction("Index");
    }

    // [HttpGet("edit-event/{id:int}")] означає:
    // - HTTP метод: GET;
    // - підмаршрут "edit-event/{id}" відносно [Route("admin")].
    // Підсумковий URL цієї дії: GET /admin/edit-event/{id}
    // Сторінка відображення форми для редагування конкретної події.
    [HttpGet("edit-event/{id:int}")]
    public async Task<IActionResult> EditEvent(int id)
    {
        var ev = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound("Подію не знайдено у базі даних.");
        }
        return View(ev);
    }

    // [HttpPost("edit-event/{id:int}")] означає:
    // - HTTP метод: POST;
    // - підмаршрут "edit-event/{id}" відносно [Route("admin")].
    // Підсумковий URL цієї дії: POST /admin/edit-event/{id}
    // Зберігає оновлені текстові поля події назад до бази даних.
    [HttpPost("edit-event/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, Event modifiedEvent)
    {
        if (id != modifiedEvent.Id)
        {
            return BadRequest();
        }

        var dbEvent = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (dbEvent == null)
        {
            return NotFound("Подію для оновлення не знайдено.");
        }

        // Оновлюємо текстові поля, які є у вашій моделі Event
        dbEvent.Title = modifiedEvent.Title;
        dbEvent.Description = modifiedEvent.Description;

        _dbContext.Events.Update(dbEvent);
        await _dbContext.SaveChangesAsync(); // Зберігаємо оновлені дані

        return RedirectToAction(nameof(Index));
    }

    // [HttpPost("delete-event/{id:int}")] означає:
    // - HTTP метод: POST;
    // - підмаршрут "delete-event/{id}" відносно [Route("admin")].
    // Підсумковий URL цієї дії: POST /admin/delete-event/{id}
    // Повністю видаляє вибрану подію з бази даних за її унікальним ID.
    [HttpPost("delete-event/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev == null)
        {
            return NotFound("Подію для видалення не знайдено.");
        }

        _dbContext.Events.Remove(ev);
        await _dbContext.SaveChangesAsync(); // Видаляємо подію з бази

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("make-organizer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeOrganizer(string userId)
    {
        // Видача ролі Organizer конкретному користувачу із таблиці.
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            user.Role = "Organizer";
            await _dbContext.SaveChangesAsync();
        }

        TempData["AdminMessage"] = "Роль оновлено";
        return RedirectToAction(nameof(Index));
    }

    private async Task FillUsers(AdminPageViewModel model)
    {
        // Підтягує список користувачів для таблиці на сторінці /admin.
        model.Users = await _dbContext.Users
            .OrderBy(x => x.Id)
            .Select(x => new AdminUserRowViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Role = x.Role
            })
            .ToListAsync();
    }
}