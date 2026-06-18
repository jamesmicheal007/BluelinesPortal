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
    public class MakePaymentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public MakePaymentModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public StudentApplication ActiveApplication { get; set; }
        public decimal TotalPaidSoFar { get; set; }
        public decimal RemainingBalance { get; set; }

        [BindProperty] public decimal AmountToPay { get; set; }
        [BindProperty] public string PaymentMethod { get; set; }
        [BindProperty] public string UTRReference { get; set; }
        [BindProperty] public IFormFile PaymentScreenshot { get; set; }

        public async Task<IActionResult> OnGetAsync(int applicationId)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            ActiveApplication = await _context.Applications
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentProfileId == profile.Id);

            if (ActiveApplication == null) return RedirectToPage("/Dashboard/Index");

            TotalPaidSoFar = await _context.Payments
                .Where(p => p.StudentApplicationId == ActiveApplication.Id && p.PaymentStatus == "Success")
                .SumAsync(p => p.AmountPaid);

            RemainingBalance = ActiveApplication.Program.BaseFee - TotalPaidSoFar;

            // Default the input box to the remaining balance
            AmountToPay = RemainingBalance;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int applicationId)
        {
            if (AmountToPay <= 0 || PaymentScreenshot == null || string.IsNullOrEmpty(UTRReference))
            {
                ModelState.AddModelError(string.Empty, "Please provide a valid amount, UTR reference, and upload the payment screenshot.");
                return await OnGetAsync(applicationId);
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts", "student_submissions");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(PaymentScreenshot.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file type. Only JPG, PNG, and PDF are allowed.");
                return await OnGetAsync(applicationId);
            }

            var uniqueFileName = Guid.NewGuid().ToString("N") + extension;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await PaymentScreenshot.CopyToAsync(fileStream);
            }

            var newPayment = new PaymentRecord
            {
                StudentApplicationId = applicationId,
                AmountPaid = AmountToPay,
                PaymentMethod = PaymentMethod,
                PaymentGatewayReference = UTRReference,
                ScreenshotPath = "/uploads/receipts/student_submissions/" + uniqueFileName,
                PaymentDate = DateTime.UtcNow,

                // CRITICAL: It is marked as Pending until the Admin verifies it!
                PaymentStatus = "PendingVerification"
            };

            _context.Payments.Add(newPayment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment details submitted successfully! Your workspace will unlock once an admin verifies the transaction.";
            return RedirectToPage("/Dashboard/Index");
        }
    }
}