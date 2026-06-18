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
    public class LearnModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LearnModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public StudentApplication ActiveApplication { get; set; }
        public IList<ProjectMaterial> CourseMaterials { get; set; }
        public bool IsFullyPaid { get; set; }

        public async Task<IActionResult> OnGetAsync(int programId)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // --- FIX: Using the newly renamed ProgramItemId ---
            ActiveApplication = await _context.Applications
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.StudentProfileId == profile.Id
                                       && a.ProgramItemId == programId
                                       && (a.Status == ApplicationStatus.Enrolled || a.Status == ApplicationStatus.Approved));

            if (ActiveApplication == null) return RedirectToPage("/Dashboard/Index");

            // Check if they have full access
            IsFullyPaid = ActiveApplication.Status == ApplicationStatus.Enrolled;

            // Fetch the downloadable assets
            CourseMaterials = await _context.ProjectMaterials
                .Where(m => m.ProgramItemId == programId)
                .OrderBy(m => m.AssetType)
                .ToListAsync();

            return Page();
        }
    }
}