using BluelinesPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BluelinesPortal.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- TOP LEVEL METRICS ---
        public decimal TotalRevenue { get; set; }
        public int TotalStudents { get; set; }
        public int PendingVerifications { get; set; }
        public int ActiveInternships { get; set; }

        // --- CHART DATA (Serialized for JavaScript) ---
        public string ChartLabelsJson { get; set; }
        public string ChartDataJson { get; set; }

        public async Task OnGetAsync()
        {
            // 1. Calculate Top Metrics
            TotalRevenue = await _context.ProductOrders
                .Where(o => o.OrderStatus == "Success" || o.OrderStatus == "Paid")
                .SumAsync(o => o.AmountPaid);

            TotalStudents = await _context.StudentProfiles.CountAsync();

            PendingVerifications = await _context.ProductOrders
                .CountAsync(o => o.OrderStatus == "PendingVerification" || o.BalanceStatus == "PendingVerification");

            ActiveInternships = await _context.Applications
                .CountAsync(a => a.Status == Models.ApplicationStatus.Enrolled);

            // 2. Generate 30-Day Revenue Chart Data
            var startDate = DateTime.UtcNow.AddDays(-30).Date;

            // Fetch only successful orders from the last 30 days
            var recentOrders = await _context.ProductOrders
                .Where(o => o.OrderDate >= startDate && (o.OrderStatus == "Success" || o.OrderStatus == "Paid"))
                .Select(o => new { o.OrderDate, o.AmountPaid })
                .ToListAsync();

            var labels = new List<string>();
            var revenueData = new List<decimal>();

            // Loop through each of the last 30 days to ensure flat days show as ₹0
            for (int i = 0; i <= 30; i++)
            {
                var targetDate = startDate.AddDays(i);
                labels.Add(targetDate.ToString("MMM dd")); // e.g., "Jun 12"

                // Sum revenue for this specific day
                var dailyTotal = recentOrders
                    .Where(o => o.OrderDate.Date == targetDate)
                    .Sum(o => o.AmountPaid);

                revenueData.Add(dailyTotal);
            }

            // Convert C# Lists to JSON strings so Chart.js can read them in the HTML
            ChartLabelsJson = JsonSerializer.Serialize(labels);
            ChartDataJson = JsonSerializer.Serialize(revenueData);
        }
    }
}