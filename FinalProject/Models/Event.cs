using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

// Подія, яка відображається на головній і в деталях.
// Capacity змінюється при бронюванні/скасуванні броні.
public class Event
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(220)]
    public string TitleDescription { get; set; } = string.Empty;

    [Required]
    [StringLength(180)]
    public string Location { get; set; } = string.Empty;

    public DateTime StartAt { get; set; }
    public int Capacity { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public string OrganizerId { get; set; } = string.Empty;
    public User? Organizer { get; set; }

    public List<SavedEvent> SavedByUsers { get; set; } = new List<SavedEvent>();
}

