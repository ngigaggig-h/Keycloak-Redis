using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

// Категорія подій для фільтрації та групування.
public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Slug { get; set; } = string.Empty;

    public List<Event> Events { get; set; } = new();
}
