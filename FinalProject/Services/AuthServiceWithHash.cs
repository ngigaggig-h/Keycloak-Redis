using FinalProject.Data;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Services;

// Сервіс авторизації.
// Містить бізнес-логіку реєстрації та входу:
// - перевірка унікальності email;
// - хешування пароля з сіллю;
// - перевірка пароля при логіні.
public class AuthServiceWithHash : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHashService _hashService;

    public AuthServiceWithHash(ApplicationDbContext dbContext, IHashService hashService)
    {
        _dbContext = dbContext;
        _hashService = hashService;
    }

    public async Task<(bool Success, string ErrorMessage, User? CreatedUser)> Register(RegisterViewModel model)
    {
        // Зводимо email до нижнього регістру для уникнення дублікатів.
        var normalizedEmail = model.Email.ToLower();

        if (await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
        {
            return (false, "Користувач з таким email вже існує", null);
        }

        var salt = _hashService.GenerateSalt();
        var hash = _hashService.BuildHash(model.Password, salt);

        var newUser = new User
        {
            Name = model.Name.Trim(),
            Email = normalizedEmail,
            Salt = salt,
            PasswordHash = hash,
            Role = "User"
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        return (true, string.Empty, newUser);
    }

    public async Task<User?> Login(LoginViewModel model)
    {
        // Шукаємо користувача і перевіряємо пароль через хеш-сервіс.
        var normalizedEmail = model.Email.ToLower();
        var user = await _dbContext.Users.FirstOrDefaultAsync(currentUser => currentUser.Email == normalizedEmail);
        if (user == null)
        {
            return null;
        }

        var isValid = _hashService.Verify(model.Password, user.Salt, user.PasswordHash);

        if (isValid)
        {
            return user;
        }

        return null;
    }
}
