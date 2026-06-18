using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Finance
{
    [Authorize(Roles = "Admin")]
    public class ManualEntryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ManualEntryModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty] public PaymentRecord NewPayment { get; set; }
        [BindProperty] public IFormFile PaymentScreenshot { get; set; }

        public SelectList ApprovedApplications { get; set; }

        public async Task OnGetAsync()
        {
            // Calculate balances dynamically inside SQL Server
            var eligibleApps = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .Where(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Enrolled)
                .Select(a => new
                {
                    Application = a,
                    TotalPaid = _context.Payments
                        .Where(p => p.StudentApplicationId == a.Id && p.PaymentStatus == "Success")
                        .Sum(p => p.AmountPaid)
                })
                .Where(x => x.TotalPaid < x.Application.Program.BaseFee) // Hide if fully paid
                .Select(x => new
                {
                    Id = x.Application.Id,
                    DisplayText = $"[{x.Application.Student.StudentId ?? "NEW"}] {x.Application.Student.FullName} - {x.Application.Program.Title} (Balance: ₹{x.Application.Program.BaseFee - x.TotalPaid:0.00})"
                })
                .ToListAsync();

            ApprovedApplications = new SelectList(eligibleApps, "Id", "DisplayText");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if ((NewPayment.PaymentMethod == "GPay" || NewPayment.PaymentMethod == "Account Transfer") && PaymentScreenshot != null)
            {
                // 1. White-list allowed extensions
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".webp" };
                var extension = Path.GetExtension(PaymentScreenshot.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "Invalid file type. Only JPG, PNG, and PDF are allowed.");
                    return Page(); // Halt execution
                }
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(PaymentScreenshot.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await PaymentScreenshot.CopyToAsync(fileStream); }
                NewPayment.ScreenshotPath = "/uploads/receipts/" + uniqueFileName;
            }

            NewPayment.PaymentDate = DateTime.UtcNow;
            NewPayment.PaymentStatus = "Success";
            NewPayment.PaymentGatewayReference = "MANUAL-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            if (NewPayment.ScreenshotPath == null) NewPayment.ScreenshotPath = "";

            _context.Payments.Add(NewPayment);

            var application = await _context.Applications.FindAsync(NewPayment.StudentApplicationId);
            if (application != null && application.Status == ApplicationStatus.Approved)
            {
                application.Status = ApplicationStatus.Enrolled;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Receipt", new { id = NewPayment.Id });
        }
    }
}