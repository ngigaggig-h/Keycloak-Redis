namespace FinalProject.DTOs;

// DTO категорії для форм і фільтрів.
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
