using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Programs
{
    [Authorize(Roles = "Admin")]
    public class MaterialsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MaterialsModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public ProgramItem ProgramDetails { get; set; }
        public IList<ProjectMaterial> ExistingMaterials { get; set; }

        [BindProperty] public ProjectMaterial NewMaterial { get; set; }
        [BindProperty] public IFormFile? FileUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ProgramDetails = await _context.Programs.FirstOrDefaultAsync(p => p.Id == id);
            if (ProgramDetails == null) return NotFound();

            ExistingMaterials = await _context.ProjectMaterials
                .Where(m => m.ProgramItemId == id)
                .OrderBy(m => m.AssetType)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(int id)
        {
            var program = await _context.Programs.FindAsync(id);
            if (program == null) return NotFound();

            NewMaterial.ProgramItemId = id;

            // Handle Physical File Upload
            if (FileUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "materials", id.ToString());
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + FileUpload.FileName.Replace(" ", "_");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(fileStream);
                }

                NewMaterial.FilePath = $"/uploads/materials/{id}/{uniqueFileName}";
            }

            _context.ProjectMaterials.Add(NewMaterial);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{NewMaterial.Title} was uploaded successfully!";
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int materialId, int programId)
        {
            var material = await _context.ProjectMaterials.FindAsync(materialId);
            if (material != null)
            {
                // Delete physical file if it exists
                if (!string.IsNullOrEmpty(material.FilePath))
                {
                    string physicalPath = Path.Combine(_env.WebRootPath, material.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                _context.ProjectMaterials.Remove(material);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Material deleted successfully.";
            }

            return RedirectToPage(new { id = programId });
        }
    }
}