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
    public class ProjectRoomModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectRoomModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public StudentApplication ActiveProject { get; set; }
        public IList<Submission> SubmissionHistory { get; set; }

        [BindProperty]
        public Submission NewSubmission { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // Verify they are enrolled in THIS specific project
            ActiveProject = await _context.Applications
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentProfileId == profile.Id && a.Status == ApplicationStatus.Enrolled);

            if (ActiveProject == null)
            {
                TempData["ErrorMessage"] = "You do not have access to this workspace.";
                return RedirectToPage("./Index");
            }

            // Fetch their past submissions for this project
            SubmissionHistory = await _context.Submissions
                .Where(s => s.StudentApplicationId == id)
                .OrderByDescending(s => s.SubmittedOn)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Security check before saving
            var isAuthorized = await _context.Applications
                .AnyAsync(a => a.Id == id && a.StudentProfileId == profile.Id && a.Status == ApplicationStatus.Enrolled);

            if (!isAuthorized) return RedirectToPage("./Index");

            // Setup the new submission record
            NewSubmission.StudentApplicationId = id;
            NewSubmission.SubmittedOn = DateTime.UtcNow;
            NewSubmission.ReviewStatus = "Pending Review";

            // Fix the .NET 8 Nullable crash we saw earlier by providing default empty strings
            NewSubmission.MentorFeedback = "";
            if (NewSubmission.CloudDriveLink == null) NewSubmission.CloudDriveLink = "";
            if (NewSubmission.GitHubLink == null) NewSubmission.GitHubLink = "";
            if (NewSubmission.StudentNotes == null) NewSubmission.StudentNotes = "";

            _context.Submissions.Add(NewSubmission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your work has been submitted successfully for mentor review!";

            // Reload the page to show the new submission
            return RedirectToPage(new { id = id });
        }
    }
}