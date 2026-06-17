using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalActivePrograms { get; set; }
        public int TotalRegisteredStudents { get; set; }
        public int TotalActiveEnrollments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // === THE TRAFFIC COP LOGIC ===
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // 1. If it's the Admin, send them to the Analytics Dashboard
                if (User.IsInRole("Admin"))
                {
                    return RedirectToPage("/Admin/Index");
                }

                // 2. If it's a Student, check if they finished setting up their profile
                var userId = _userManager.GetUserId(User);
                var hasProfile = await _context.StudentProfiles.AnyAsync(p => p.IdentityUserId == userId);

                if (!hasProfile)
                {
                    return RedirectToPage("/Student/ProfileSetup");
                }

                // 3. Profile is complete, send them to the Student Dashboard
                return RedirectToPage("/Dashboard/Index");
            }

            // === GUEST LOGIC (Only runs if nobody is logged in) ===
            TotalActivePrograms = await _context.Programs.CountAsync(p => p.IsActive);
            TotalRegisteredStudents = await _context.StudentProfiles.CountAsync() + 150;
            TotalActiveEnrollments = await _context.Applications.CountAsync(a => a.Status == ApplicationStatus.Enrolled) + 85;

            return Page();
        }
    }
}