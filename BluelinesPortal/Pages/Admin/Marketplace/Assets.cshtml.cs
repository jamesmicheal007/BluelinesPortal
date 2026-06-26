using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Marketplace
{
    [Authorize(Roles = "Admin")]
    public class AssetsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssetsModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public DigitalProduct ProductDetails { get; set; }
        public IList<ProductAsset> ExistingAssets { get; set; }

        [BindProperty] public ProductAsset NewAsset { get; set; }
        [BindProperty] public IFormFile? FileUpload { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ProductDetails = await _context.DigitalProducts.FirstOrDefaultAsync(p => p.Id == id);
            if (ProductDetails == null) return NotFound();

            ExistingAssets = await _context.ProductAssets
                .Where(m => m.DigitalProductId == id)
                .OrderBy(m => m.Title)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadAsync(int id)
        {
            var product = await _context.DigitalProducts.FindAsync(id);
            if (product == null) return NotFound();

            NewAsset.DigitalProductId = id;

            // Handle Physical File Upload
            if (FileUpload != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "marketplace", "assets", id.ToString());
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString("N") + "_" + FileUpload.FileName.Replace(" ", "_");
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(fileStream);
                }

                NewAsset.FilePath = $"/uploads/marketplace/assets/{id}/{uniqueFileName}";
            }

            _context.ProductAssets.Add(NewAsset);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{NewAsset.Title} was uploaded successfully!";
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int assetId, int productId)
        {
            var asset = await _context.ProductAssets.FindAsync(assetId);
            if (asset != null)
            {
                // Delete physical file if it exists
                if (!string.IsNullOrEmpty(asset.FilePath))
                {
                    string physicalPath = Path.Combine(_env.WebRootPath, asset.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                _context.ProductAssets.Remove(asset);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Asset deleted successfully.";
            }

            return RedirectToPage(new { id = productId });
        }
    }
}