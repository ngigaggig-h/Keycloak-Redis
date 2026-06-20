namespace FinalProject.Services.Interfaces;

// Контракт сервісу для роботи з паролями.
public interface IHashService
{
    string GenerateSalt();
    string BuildHash(string password, string salt);
    bool Verify(string password, string salt, string savedHash);
}
