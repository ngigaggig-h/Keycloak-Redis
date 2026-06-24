using System.Collections.Generic;

namespace FinalProject.ViewModels;

public class DashboardViewModel
{
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int TotalCapacity { get; set; }

    // Словник для збереження результатів LINQ-групування GroupBy
    public Dictionary<string, int> EventsByLocation { get; set; } = new();
}