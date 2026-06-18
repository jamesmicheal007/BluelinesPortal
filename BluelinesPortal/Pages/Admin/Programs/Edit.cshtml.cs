using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Programs
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty] public ProgramItem EditProgram { get; set; }
        [BindProperty] public IFormFile? ThumbnailUpload { get; set; }
        [BindProperty] public IFormFile? BrochureUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();
            EditProgram = await _context.Programs.FirstOrDefaultAsync(m => m.Id == id);
            if (EditProgram == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var programToUpdate = await _context.Programs.FindAsync(EditProgram.Id);
            if (programToUpdate == null) return NotFound();

            // --- PRICING & DISCOUNT SYNC ---
            programToUpdate.BaseFee = EditProgram.BaseFee;
            programToUpdate.IsDiscountActive = EditProgram.IsDiscountActive;

            if (!EditProgram.IsDiscountActive)
            {
                programToUpdate.DiscountType = DiscountType.None;
                programToUpdate.DiscountValue = 0;
                programToUpdate.CouponCode = null;
            }
            else
            {
                programToUpdate.DiscountType = EditProgram.DiscountType;
                programToUpdate.DiscountValue = EditProgram.DiscountValue;
                programToUpdate.CouponCode = string.IsNullOrWhiteSpace(EditProgram.CouponCode) ? null : EditProgram.CouponCode.Trim().ToUpper();
            }

            // --- STANDARD UPDATES ---
            programToUpdate.Title = EditProgram.Title;
            programToUpdate.ShortDescription = EditProgram.ShortDescription;
            programToUpdate.Description = EditProgram.Description;
            programToUpdate.Type = EditProgram.Type;
            programToUpdate.DurationInDays = EditProgram.DurationInDays;
            programToUpdate.IsActive = EditProgram.IsActive;
            programToUpdate.YouTubeVideoUrl = EditProgram.YouTubeVideoUrl;
            programToUpdate.Prerequisites = EditProgram.Prerequisites;

            // --- FILE UPLOADS ---
            if (ThumbnailUpload != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "programs", "images");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(ThumbnailUpload.FileName);
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create)) { await ThumbnailUpload.CopyToAsync(stream); }
                programToUpdate.ThumbnailPath = "/uploads/programs/images/" + fileName;
            }

            if (BrochureUpload != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads", "programs", "brochures");
                Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(BrochureUpload.FileName);
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create)) { await BrochureUpload.CopyToAsync(stream); }
                programToUpdate.BrochurePath = "/uploads/programs/brochures/" + fileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}