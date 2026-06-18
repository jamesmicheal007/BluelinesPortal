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

        public ProgramItem ProgramDetails { get; set; }
        public StudentProfile CurrentProfile { get; set; }

        [BindProperty] public StudentApplication NewApplication { get; set; }

        // This grabs the coupon code from the front-end form
        [BindProperty] public string? AppliedCouponCode { get; set; }

        public async Task<IActionResult> OnGetAsync(int programId)
        {
            var userId = _userManager.GetUserId(User);
            CurrentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (CurrentProfile == null) return RedirectToPage("/Student/ProfileSetup");

            ProgramDetails = await _context.Programs.FirstOrDefaultAsync(p => p.Id == programId && p.IsActive);
            if (ProgramDetails == null) return NotFound();

            // Safety Check: Prevent applying for the same program twice
            var existingApp = await _context.Applications
                .FirstOrDefaultAsync(a => a.StudentProfileId == CurrentProfile.Id && a.ProgramItemId == programId);

            if (existingApp != null)
            {
                TempData["WarningMessage"] = "You have already applied for this program.";
                return RedirectToPage("/Dashboard/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int programId)
        {
            var userId = _userManager.GetUserId(User);
            CurrentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);
            ProgramDetails = await _context.Programs.FirstOrDefaultAsync(p => p.Id == programId);

            if (CurrentProfile == null || ProgramDetails == null) return NotFound();

            // ==========================================
            // 💡 THE SERVER-SIDE PRICING ENGINE
            // ==========================================
            decimal calculatedFee = ProgramDetails.BaseFee;

            if (ProgramDetails.IsDiscountActive)
            {
                bool isDiscountValid = false;

                // Scenario A: Discount is Automatic (No coupon code required by admin)
                if (string.IsNullOrEmpty(ProgramDetails.CouponCode))
                {
                    isDiscountValid = true;
                }
                // Scenario B: Admin requires a code, and the student typed it correctly
                else if (!string.IsNullOrEmpty(AppliedCouponCode) &&
                         AppliedCouponCode.Trim().ToUpper() == ProgramDetails.CouponCode)
                {
                    isDiscountValid = true;
                }
                // Scenario C: Admin requires a code, but the student typed it WRONG
                else if (!string.IsNullOrEmpty(AppliedCouponCode))
                {
                    ModelState.AddModelError("AppliedCouponCode", "Invalid or expired coupon code.");
                }

                // If the discount passes validation, apply the math
                if (isDiscountValid)
                {
                    if (ProgramDetails.DiscountType == DiscountType.Percentage)
                    {
                        decimal discountAmount = (ProgramDetails.BaseFee * ProgramDetails.DiscountValue) / 100;
                        calculatedFee = ProgramDetails.BaseFee - discountAmount;
                    }
                    else if (ProgramDetails.DiscountType == DiscountType.FixedAmount)
                    {
                        calculatedFee = ProgramDetails.BaseFee - ProgramDetails.DiscountValue;
                    }
                }
            }

            // Safety net: Never allow negative fees
            if (calculatedFee < 0) calculatedFee = 0;

            // ==========================================
            // 💡 FIX: CLEAR OVER-VALIDATION ERRORS
            // Tell ASP.NET to ignore these missing objects because 
            // we are manually assigning them in the code below.
            // ==========================================
            ModelState.Remove("NewApplication.Program");
            ModelState.Remove("NewApplication.Student");
            ModelState.Remove("NewApplication.StudentProfileId");
            ModelState.Remove("NewApplication.ProgramItemId");

            if (!ModelState.IsValid)
            {
                return Page(); // Reload page to show the "Invalid Coupon" error message
            }

            // Lock in the application data
            NewApplication.StudentProfileId = CurrentProfile.Id;
            NewApplication.ProgramItemId = programId;
            NewApplication.AppliedOn = DateTime.UtcNow;
            NewApplication.Status = ApplicationStatus.Pending;

            // 🔒 CRITICAL: Lock the price permanently in the ledger
            NewApplication.FinalFee = calculatedFee;

            _context.Applications.Add(NewApplication);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Application submitted successfully! Our team will review it shortly.";
            return RedirectToPage("/Dashboard/Index");
        }
    }
}