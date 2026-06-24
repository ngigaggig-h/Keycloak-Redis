using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject.Data; // Твій DbContext
using FinalProject.ViewModels;

namespace FinalProject.Controllers;

// 🔒 Доступ дозволено виключно користувачам з роллю Admin у Keycloak
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public DashboardController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 📊 1. READ & AGGREGATE: Головна сторінка дашборду
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var totalEvents = await _dbContext.Events.CountAsync();

        // Кількість бронювань квитків із таблиці BookedEvents
        var totalBookings = await _dbContext.BookedEvents.CountAsync();

        // Сума місткості всіх залів
        var totalCapacity = await _dbContext.Events.SumAsync(e => (int?)e.Capacity) ?? 0;

        // 🧩 Агрегація даних через GroupBy (групуємо події за локаціями)
        // Якщо у вашій моделі Event є поле Category, можна замінити e.Location на e.Category
        var locationStats = await _dbContext.Events
            .GroupBy(e => e.Location)
            .Select(g => new { Location = g.Key, Count = g.Count() })
            .ToDictionaryAsync(
                x => string.IsNullOrEmpty(x.Location) ? "Не вказано" : x.Location,
                x => x.Count
            );

        var viewModel = new DashboardViewModel
        {
            TotalEvents = totalEvents,
            TotalBookings = totalBookings,
            TotalCapacity = totalCapacity,
            EventsByLocation = locationStats
        };

        return View(viewModel);
    }

    // 📥 2. BONUS: Генерація CSV-файлу "на льоту" за допомогою StringBuilder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportToCsv()
    {
        var events = await _dbContext.Events.ToListAsync();

        var csvBuilder = new StringBuilder();

        // Заголовки стовпців (використовуємо крапку з комою для коректного відкриття в Excel)
        csvBuilder.AppendLine("Id;Назва події;Локація;Дата початку;Макс. місткість");

        foreach (var ev in events)
        {
            csvBuilder.AppendLine($"{ev.Id};{ev.Title};{ev.Location};{ev.StartAt:dd.MM.yyyy HH:mm};{ev.Capacity}");
        }

        // Кодування UTF-8 з BOM, щоб українські літери в Excel не перетворилися на ієрогліфи
        var fileBytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
        string fileName = $"PodiiHub_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        return File(fileBytes, "text/csv", fileName);
    }
}