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

        public StudentApplication ApprovedApplication { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // Fetch the specific application, ensuring it belongs to THIS student
            ApprovedApplication = await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentProfileId == profile.Id);

            if (ApprovedApplication == null) return NotFound();

            // Security Check: They can only be here if the admin approved it
            if (ApprovedApplication.Status != ApplicationStatus.Approved)
            {
                TempData["ErrorMessage"] = "This offer is not valid or has expired.";
                return RedirectToPage("/Dashboard/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            // Note: In a production environment, this POST method would redirect 
            // to Razorpay/Cashfree. We are simulating a successful payment return here.

            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            var applicationToUpdate = await _context.Applications
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentProfileId == profile.Id);

            if (applicationToUpdate == null || applicationToUpdate.Status != ApplicationStatus.Approved)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            // 1. Update Application Status to Enrolled
            applicationToUpdate.Status = ApplicationStatus.Enrolled;

            // 2. Generate a Payment Record for the Admin Financial Dashboard
            var paymentRecord = new PaymentRecord
            {
                StudentApplicationId = applicationToUpdate.Id,
                AmountPaid = applicationToUpdate.Program.BaseFee,
                PaymentDate = DateTime.UtcNow,
                PaymentGatewayReference = "SIM_" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(), // Simulated ID
                PaymentStatus = "Success"
            };

            _context.Payments.Add(paymentRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment successful! Welcome to the program. Your workspace is now unlocked.";
            return RedirectToPage("/Dashboard/Index");
        }
    }
}