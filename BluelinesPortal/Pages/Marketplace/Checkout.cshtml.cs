using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Marketplace
{
    [Authorize] // Forces public users to log in/register before buying
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public CheckoutModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public DigitalProduct Product { get; set; }
        public StudentProfile CurrentProfile { get; set; }

        // --- PAYMENT FIELDS ---
        [BindProperty] public string PaymentMethod { get; set; }
        [BindProperty] public string? UTRNumber { get; set; }
        [BindProperty] public IFormFile? ScreenshotUpload { get; set; }
        [BindProperty] public string PaymentPlan { get; set; } // "Full" or "Split"

        // --- ADD-ON FIELDS (Captured from the URL) ---
        [BindProperty(SupportsGet = true)] public decimal optExplanation { get; set; }
        [BindProperty(SupportsGet = true)] public decimal optInstallation { get; set; }
        [BindProperty(SupportsGet = true)] public decimal optReport { get; set; }
        [BindProperty(SupportsGet = true)] public decimal optFormatting { get; set; }
        [BindProperty(SupportsGet = true)] public decimal optCustomization { get; set; }

        // Dynamic Total Calculation
        public decimal TotalAddOnCost => optExplanation + optInstallation + optReport + optFormatting + optCustomization;

        public async Task<IActionResult> OnGetAsync(int productId)
        {
            var userId = _userManager.GetUserId(User);
            CurrentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (CurrentProfile == null) return RedirectToPage("/Student/ProfileSetup");

            Product = await _context.DigitalProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);

            if (Product == null) return NotFound();

            // Prevent buying the same project twice
            var existingOrder = await _context.ProductOrders
                .FirstOrDefaultAsync(o => o.StudentProfileId == CurrentProfile.Id && o.DigitalProductId == productId);

            if (existingOrder != null)
            {
                if (existingOrder.OrderStatus == "Success")
                {
                    TempData["SuccessMessage"] = "You already own this project! You can download it from your digital library.";
                    return RedirectToPage("/Workspace/Library");
                }
                else if (existingOrder.OrderStatus == "PendingVerification")
                {
                    TempData["WarningMessage"] = "You have already submitted a payment for this project. Please wait for Admin verification.";
                    return RedirectToPage("/Dashboard/Index");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int productId)
        {
            var userId = _userManager.GetUserId(User);
            CurrentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);
            Product = await _context.DigitalProducts.FirstOrDefaultAsync(p => p.Id == productId);

            if (CurrentProfile == null || Product == null) return NotFound();

            // ==========================================
            // 💡 ADVANCED PRICING & SPLIT PAYMENT LOGIC
            // ==========================================
            decimal finalGrandTotal = Product.Price + TotalAddOnCost;
            decimal amountToPayNow = finalGrandTotal;
            decimal balanceRemaining = 0;
            bool isSplit = false;

            // If they chose the 50% split option
            if (PaymentPlan == "Split" && finalGrandTotal > 0)
            {
                isSplit = true;
                amountToPayNow = finalGrandTotal / 2;
                balanceRemaining = finalGrandTotal - amountToPayNow;
            }

            // Build a clean string of the Add-ons they selected
            var selectedAddOnsList = new List<string>();
            if (optExplanation > 0) selectedAddOnsList.Add("Explanation");
            if (optInstallation > 0) selectedAddOnsList.Add("Installation");
            if (optReport > 0) selectedAddOnsList.Add("Standard Report");
            if (optFormatting > 0) selectedAddOnsList.Add("Report Formatting");
            if (optCustomization > 0) selectedAddOnsList.Add("Customization");

            string addOnString = selectedAddOnsList.Any() ? string.Join(", ", selectedAddOnsList) : "None";

            var newOrder = new ProductOrder
            {
                StudentProfileId = CurrentProfile.Id,
                DigitalProductId = Product.Id,
                OrderDate = DateTime.UtcNow,
                PaymentMethod = finalGrandTotal == 0 ? "Free Download" : "UPI",
                UTRNumber = UTRNumber,
                OrderStatus = finalGrandTotal == 0 ? "Success" : "PendingVerification", // Free items are instant!

                // Inject the new DB tracking fields
                AmountPaid = amountToPayNow,
                SelectedAddOns = addOnString,
                AddOnTotal = TotalAddOnCost,
                IsSplitPayment = isSplit,
                BalanceDue = balanceRemaining
            };

            // Handle File Upload if the total cost is greater than zero
            if (finalGrandTotal > 0)
            {
                if (ScreenshotUpload == null || string.IsNullOrWhiteSpace(UTRNumber))
                {
                    ModelState.AddModelError(string.Empty, "Please provide the UTR number and upload your payment screenshot.");
                    return await OnGetAsync(productId);
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(ScreenshotUpload.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, "Invalid file type. Only JPG, PNG, and PDF are allowed.");
                    return await OnGetAsync(productId);
                }

                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts", "marketplace");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString("N") + extension;

                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                {
                    await ScreenshotUpload.CopyToAsync(fileStream);
                }

                newOrder.ScreenshotPath = "/uploads/receipts/marketplace/" + uniqueFileName;
            }

            _context.ProductOrders.Add(newOrder);
            await _context.SaveChangesAsync();

            if (finalGrandTotal == 0)
            {
                TempData["SuccessMessage"] = "Project claimed successfully! You can download it now.";
            }
            else
            {
                TempData["SuccessMessage"] = isSplit
                    ? $"50% Advance Payment of ₹{amountToPayNow:0} submitted! Access will be granted upon Admin verification."
                    : "Full Payment submitted successfully! You will gain access as soon as our team verifies the screenshot.";
            }

            return RedirectToPage("/Dashboard/Index");
        }
    }
}