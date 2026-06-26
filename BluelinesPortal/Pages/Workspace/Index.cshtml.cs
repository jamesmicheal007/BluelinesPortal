using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Workspace
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public StudentApplication ActiveApplication { get; set; }
        public IList<Submission> PastSubmissions { get; set; }
        public bool IsProjectApproved { get; set; }

        [BindProperty]
        public Submission NewSubmission { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // Fetch the specific enrollment, or just grab their latest active one
            var appQuery = _context.Applications
                .Include(a => a.Program)
                .Where(a => a.StudentProfileId == profile.Id && a.Status == ApplicationStatus.Enrolled);

            if (id.HasValue)
            {
                ActiveApplication = await appQuery.FirstOrDefaultAsync(a => a.Id == id.Value);
            }
            else
            {
                ActiveApplication = await appQuery.OrderByDescending(a => a.AppliedOn).FirstOrDefaultAsync();
            }

            // If they have no active enrollments, send them back to the dashboard
            if (ActiveApplication == null) return RedirectToPage("/Dashboard/Index");

            // Fetch their submission history
            PastSubmissions = await _context.Submissions
                .Where(s => s.StudentApplicationId == ActiveApplication.Id)
                .OrderByDescending(s => s.SubmittedOn)
                .ToListAsync();

            // Check if they have officially passed the program
            IsProjectApproved = PastSubmissions.Any(s => s.ReviewStatus == "Approved");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // ==========================================
            // 💡 FIX: CLEAR OVER-VALIDATION ERRORS
            // Tell ASP.NET to ignore properties that the student 
            // isn't supposed to fill out in the HTML form!
            // ==========================================
            ModelState.Remove("NewSubmission.Application");
            ModelState.Remove("NewSubmission.MentorFeedback");
            ModelState.Remove("NewSubmission.ReviewStatus");

            if (!ModelState.IsValid)
            {
                // If it still fails, reload the page
                return await OnGetAsync(NewSubmission.StudentApplicationId);
            }

            // Lock in the server-side timestamps and default status
            NewSubmission.SubmittedOn = DateTime.UtcNow;
            NewSubmission.ReviewStatus = "Pending";

            // Prevent SQL Null Crashes for optional fields
            NewSubmission.MentorFeedback = "";
            if (NewSubmission.CloudDriveLink == null) NewSubmission.CloudDriveLink = "";
            if (NewSubmission.StudentNotes == null) NewSubmission.StudentNotes = "";

            _context.Submissions.Add(NewSubmission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your work has been submitted successfully! Your mentor is notified and will review it shortly.";
            return RedirectToPage(new { id = NewSubmission.StudentApplicationId });
        }
    }
}