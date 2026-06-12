using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels;

// ViewModel сторінки /admin:
// поля для створення події + список користувачів.
public class AdminPageViewModel
{
    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(220)]
    public string TitleDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateTime? StartAt { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; } = 50;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public List<AdminUserRowViewModel> Users { get; set; } = new();
}

public class AdminUserRowViewModel
{
    // Один рядок у таблиці користувачів на адмін-сторінці.
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
