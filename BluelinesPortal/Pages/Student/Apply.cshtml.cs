using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Student
{
    [Authorize]
    public class ApplyModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApplyModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ProgramItem ProgramToApply { get; set; }
        public StudentProfile CurrentProfile { get; set; }
        public bool HasAlreadyApplied { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // 1. Fetch the active program
            ProgramToApply = await _context.Programs
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (ProgramToApply == null) return NotFound();

            // 2. Fetch the logged-in student's profile
            var userId = _userManager.GetUserId(User);
            CurrentProfile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // Traffic cop: If they bypassed setup, route them back
            if (CurrentProfile == null) return RedirectToPage("/Student/ProfileSetup");

            // 3. Prevent duplicate applications
            HasAlreadyApplied = await _context.Applications
                .AnyAsync(a => a.ProgramItemId == id && a.StudentProfileId == CurrentProfile.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // Double-check on POST to prevent duplicate submission via refresh
            var existingApp = await _context.Applications
                .AnyAsync(a => a.ProgramItemId == id && a.StudentProfileId == profile.Id);

            if (existingApp)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            // Create the new application record
            var newApplication = new StudentApplication
            {
                StudentProfileId = profile.Id,
                ProgramItemId = id,
                AppliedOn = DateTime.UtcNow,
                Status = ApplicationStatus.Pending,
                AdminNotes = ""
            };

            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();

            // Set a temporary success message to display on the dashboard
            TempData["SuccessMessage"] = "Your application has been successfully submitted!";

            return RedirectToPage("/Dashboard/Index");
        }
    }
}