using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BluelinesPortal.Pages.Admin.Programs
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CreateModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty] public ProgramItem NewProgram { get; set; }
        [BindProperty] public IFormFile? ThumbnailUpload { get; set; }
        [BindProperty] public IFormFile? BrochureUpload { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // --- PRICING & DISCOUNT CLEANUP ---
            if (!NewProgram.IsDiscountActive)
            {
                NewProgram.DiscountType = DiscountType.None;
                NewProgram.DiscountValue = 0;
                NewProgram.CouponCode = null;
            }
            else
            {
                // Force coupon codes to uppercase and trim spaces
                NewProgram.CouponCode = string.IsNullOrWhiteSpace(NewProgram.CouponCode) ? null : NewProgram.CouponCode.Trim().ToUpper();
            }

            // 1. Handle File Uploads
            if (ThumbnailUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(ThumbnailUpload.FileName);
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await ThumbnailUpload.CopyToAsync(fileStream); }
                NewProgram.ThumbnailPath = "/uploads/programs/images/" + uniqueFileName;
            }

            if (BrochureUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "brochures");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(BrochureUpload.FileName);
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await BrochureUpload.CopyToAsync(fileStream); }
                NewProgram.BrochurePath = "/uploads/programs/brochures/" + uniqueFileName;
            }

            _context.Programs.Add(NewProgram);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}