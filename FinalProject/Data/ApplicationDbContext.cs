using FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Data;

// Контекст EF Core: через нього йде вся робота з таблицями БД.
// Використовується в сервісах і контролерах через DI.
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<SavedEvent> SavedEvents => Set<SavedEvent>();
    public DbSet<BookedEvent> BookedEvents => Set<BookedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Вимикаємо каскадне видалення для BookedEvents (зв'язок з User)
        modelBuilder.Entity<BookedEvent>()
            .HasOne(be => be.User)
            .WithMany() // або .WithMany(u => u.BookedEvents), якщо у вас є такий список в моделі User
            .HasForeignKey(be => be.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Забороняємо каскад

        // 2. Вимикаємо каскадне видалення для SavedEvents (зв'язок з User)
        modelBuilder.Entity<SavedEvent>()
            .HasOne(se => se.User)
            .WithMany() // або .WithMany(u => u.SavedEvents)
            .HasForeignKey(se => se.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Забороняємо каскад

        modelBuilder.Entity<Category>()
            .HasIndex(category => category.Slug)
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(eventEntity => eventEntity.Category)
            .WithMany(category => category.Events)
            .HasForeignKey(eventEntity => eventEntity.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
