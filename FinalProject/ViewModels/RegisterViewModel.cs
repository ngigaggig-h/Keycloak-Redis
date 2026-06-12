using System.ComponentModel.DataAnnotations;

namespace FinalProject.ViewModels;

// Модель форми реєстрації.
public class RegisterViewModel
{
    [Required(ErrorMessage = "Вкажіть ім'я")]
    [StringLength(100, ErrorMessage = "Ім'я має бути до 100 символів")]
    [Display(Name = "Ім'я")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть email")]
    [EmailAddress(ErrorMessage = "Некоректний формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Вкажіть пароль")]
    [MinLength(6, ErrorMessage = "Пароль має містити щонайменше 6 символів")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Підтвердіть пароль")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Паролі не співпадають")]
    [Display(Name = "Підтвердження пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
