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
    public class AcceptOfferModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AcceptOfferModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // 💡 FIX: Renamed back to 'ApprovedApplication' to match your HTML
        // ==========================================
        public StudentApplication ApprovedApplication { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            ApprovedApplication = await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id && a.Student.IdentityUserId == userId);

            // Only allow access if the application exists and is currently Approved
            if (ApprovedApplication == null || ApprovedApplication.Status != ApplicationStatus.Approved)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = _userManager.GetUserId(User);

            var application = await _context.Applications
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id && a.Student.IdentityUserId == userId);

            if (application == null || application.Status != ApplicationStatus.Approved)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            if (application.FinalFee <= 0)
            {
                // SCENARIO A: 100% Scholarship or Free Program
                // Auto-enroll them immediately
                application.Status = ApplicationStatus.Enrolled;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Offer accepted! Since this program is fully covered, you have been instantly enrolled.";
                return RedirectToPage("/Workspace/Index", new { id = application.Id });
            }
            else
            {
                // SCENARIO B: Payment is required
                // Keep status as 'Approved' and send them to the payment gateway
                return RedirectToPage("/Student/MakePayment", new { applicationId = application.Id });
            }
        }
    }
}