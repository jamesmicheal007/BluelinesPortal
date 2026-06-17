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
        [BindProperty] public IFormFile ThumbnailUpload { get; set; }
        [BindProperty] public IFormFile BrochureUpload { get; set; }

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

            // 1. Handle File Uploads (Only replace if a new file is selected)
            if (ThumbnailUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ThumbnailUpload.FileName;
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await ThumbnailUpload.CopyToAsync(fileStream); }
                programToUpdate.ThumbnailPath = "/uploads/programs/images/" + uniqueFileName;
            }

            if (BrochureUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "brochures");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + BrochureUpload.FileName;
                using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) { await BrochureUpload.CopyToAsync(fileStream); }
                programToUpdate.BrochurePath = "/uploads/programs/brochures/" + uniqueFileName;
            }

            // 2. Update Text Fields
            programToUpdate.Title = EditProgram.Title;
            programToUpdate.ShortDescription = EditProgram.ShortDescription;
            programToUpdate.Description = EditProgram.Description;
            programToUpdate.Type = EditProgram.Type;
            programToUpdate.DurationInDays = EditProgram.DurationInDays;
            programToUpdate.BaseFee = EditProgram.BaseFee;
            programToUpdate.IsActive = EditProgram.IsActive;
            programToUpdate.YouTubeVideoUrl = EditProgram.YouTubeVideoUrl;
            programToUpdate.Prerequisites = EditProgram.Prerequisites;

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}