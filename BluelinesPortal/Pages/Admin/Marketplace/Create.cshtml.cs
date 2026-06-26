using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Marketplace
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

        [BindProperty] public DigitalProduct NewProduct { get; set; }
        [BindProperty] public IFormFile? ThumbnailUpload { get; set; }

        public SelectList CategoryList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Populate the Dropdown with categories
            var categories = await _context.ProductCategories.Where(c => c.IsActive).ToListAsync();
            CategoryList = new SelectList(categories, "Id", "Name");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (NewProduct.Price <= 0)
            {
                NewProduct.Price = 0;
                NewProduct.IsFree = true;
            }

            // Secure File Upload (Out of wwwroot if you want to protect it, but thumbnails are public)
            if (ThumbnailUpload != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(ThumbnailUpload.FileName).ToLowerInvariant();

                if (allowedExtensions.Contains(extension))
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "marketplace", "thumbnails");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString("N") + extension;
                    using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create))
                    {
                        await ThumbnailUpload.CopyToAsync(fileStream);
                    }
                    NewProduct.ThumbnailPath = "/uploads/marketplace/thumbnails/" + uniqueFileName;
                }
            }

            NewProduct.CreatedOn = DateTime.UtcNow;

            _context.DigitalProducts.Add(NewProduct);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{NewProduct.Title} has been successfully listed in the store!";
            return RedirectToPage("./Index");
        }
    }
}