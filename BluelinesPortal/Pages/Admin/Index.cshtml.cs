using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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

        // Top Level Metrics
        public int TotalStudents { get; set; }
        public int TotalApplications { get; set; }
        public decimal TotalRevenue { get; set; }

        // Data for Pie Chart (Application Statuses)
        public List<string> StatusLabels { get; set; } = new List<string>();
        public List<int> StatusCounts { get; set; } = new List<int>();

        // Data for Bar Chart (Recent Revenue)
        public List<string> RevenueLabels { get; set; } = new List<string>();
        public List<decimal> RevenueData { get; set; } = new List<decimal>();

        public async Task OnGetAsync()
        {
            // 1. Calculate Top Level Metrics
            TotalStudents = await _context.StudentProfiles.CountAsync();
            TotalApplications = await _context.Applications.CountAsync();
            TotalRevenue = await _context.Payments.SumAsync(p => p.AmountPaid);

            // 2. Prepare Data for the Pie Chart (Group by Status)
            var statusGroups = await _context.Applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            foreach (var group in statusGroups)
            {
                StatusLabels.Add(group.Status);
                StatusCounts.Add(group.Count);
            }

            // 3. Prepare Data for the Bar Chart (Last 7 Days Revenue)
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-6).Date;

            var dailyRevenue = await _context.Payments
                .Where(p => p.PaymentDate >= sevenDaysAgo)
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(p => p.AmountPaid) })
                .ToListAsync();

            // Ensure we have a label for all 7 days, even if revenue was 0
            for (int i = 0; i < 7; i++)
            {
                var targetDate = sevenDaysAgo.AddDays(i);
                RevenueLabels.Add(targetDate.ToString("dd MMM"));

                var dayData = dailyRevenue.FirstOrDefault(d => d.Date == targetDate);
                RevenueData.Add(dayData != null ? dayData.Total : 0);
            }
        }
    }
}