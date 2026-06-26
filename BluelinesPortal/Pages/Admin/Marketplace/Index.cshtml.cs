using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Marketplace
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<DigitalProduct> Products { get; set; }
        public IList<ProductCategory> Categories { get; set; }

        [BindProperty]
        public string NewCategoryName { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch products for the grid (Read-Only)
            Products = await _context.DigitalProducts
                .Include(p => p.Category)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();

            // Fetch categories for the stats and dropdowns
            Categories = await _context.ProductCategories
                .AsNoTracking()
                .ToListAsync();
        }

        // Quick action to add a new category without leaving the page
        public async Task<IActionResult> OnPostCreateCategoryAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewCategoryName))
            {
                var category = new ProductCategory { Name = NewCategoryName.Trim() };
                _context.ProductCategories.Add(category);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Category '{category.Name}' was successfully created!";
            }
            return RedirectToPage();
        }
    }
}