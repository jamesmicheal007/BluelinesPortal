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
        [BindProperty] public IFormFile ThumbnailUpload { get; set; }
        [BindProperty] public IFormFile BrochureUpload { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Handle File Uploads
            if (ThumbnailUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "images");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + ThumbnailUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await ThumbnailUpload.CopyToAsync(fileStream); }
                NewProgram.ThumbnailPath = "/uploads/programs/images/" + uniqueFileName;
            }

            if (BrochureUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "programs", "brochures");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + BrochureUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create)) { await BrochureUpload.CopyToAsync(fileStream); }
                NewProgram.BrochurePath = "/uploads/programs/brochures/" + uniqueFileName;
            }

            _context.Programs.Add(NewProgram);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}