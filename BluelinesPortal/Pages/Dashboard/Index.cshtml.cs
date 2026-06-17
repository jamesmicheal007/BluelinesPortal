using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Dashboard
{
    [Authorize] // Only logged-in users
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public StudentProfile CurrentStudent { get; set; }
        public IList<StudentApplication> MyApplications { get; set; }

        // Data for the Analytics Chart
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<int> ChartData { get; set; } = new List<int>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            // 1. Get the student's profile
            CurrentStudent = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Failsafe: If they somehow bypassed the setup page, send them back
            if (CurrentStudent == null)
            {
                return RedirectToPage("/Student/ProfileSetup");
            }

            // 2. Get all applications made by this student
            MyApplications = await _context.Applications
                .Include(a => a.Program)
                .Where(a => a.StudentProfileId == CurrentStudent.Id)
                .OrderByDescending(a => a.AppliedOn)
                .ToListAsync();

            // 3. Prepare data for the Status Chart
            var statusGroups = MyApplications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToList();

            foreach (var group in statusGroups)
            {
                // Add spaces before capital letters for better chart labels (e.g., "UnderReview" -> "Under Review")
                string label = System.Text.RegularExpressions.Regex.Replace(group.Status, "([A-Z])", " $1").Trim();
                ChartLabels.Add(label);
                ChartData.Add(group.Count);
            }

            return Page();
        }
    }
}