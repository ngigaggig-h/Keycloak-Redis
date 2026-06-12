using System.ComponentModel.DataAnnotations;

namespace FinalProject.Models;

// Користувач системи.
// Role використовується для доступу до дій (User / Organizer / Admin).
public class User
{
    public string Id { get; set; } = string.Empty;

    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Salt { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Role { get; set; } = "User";
}
