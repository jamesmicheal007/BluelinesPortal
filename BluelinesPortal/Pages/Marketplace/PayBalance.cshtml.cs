using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Marketplace
{
    [Authorize]
    public class PayBalanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PayBalanceModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        public ProductOrder TargetOrder { get; set; }

        [BindProperty] public string UTRNumber { get; set; }
        [BindProperty] public IFormFile ScreenshotUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            TargetOrder = await _context.ProductOrders
                .Include(o => o.Product)
                .Include(o => o.Student)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Student.IdentityUserId == userId);

            if (TargetOrder == null || TargetOrder.BalanceDue <= 0 || TargetOrder.BalanceStatus == "PendingVerification")
            {
                return RedirectToPage("/Workspace/Library");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int orderId)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.ProductOrders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Student.IdentityUserId == userId);

            if (order == null) return NotFound();

            if (ScreenshotUpload != null && !string.IsNullOrWhiteSpace(UTRNumber))
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(ScreenshotUpload.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "receipts", "marketplace");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString("N") + "_balance" + extension;

                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    {
                        await ScreenshotUpload.CopyToAsync(fileStream);
                    }

                    order.BalanceScreenshotPath = "/uploads/receipts/marketplace/" + uniqueFileName;
                    order.BalanceUTRNumber = UTRNumber;
                    order.BalanceStatus = "PendingVerification";

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Balance payment proof uploaded! Files will unlock once verified by Admin.";
                }
            }
            return RedirectToPage("/Workspace/Library");
        }
    }
}