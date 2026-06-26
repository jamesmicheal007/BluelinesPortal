using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Marketplace
{
    [AllowAnonymous] // Anyone can browse your store!
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<DigitalProduct> Products { get; set; }
        public IList<ProductCategory> Categories { get; set; }

        [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch active categories for the filter buttons
            Categories = await _context.ProductCategories
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync();

            // Fetch active products
            var query = _context.DigitalProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsNoTracking();

            // Apply Filters
            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(p => p.Title.Contains(SearchString) ||
                                         p.FrontendTech.Contains(SearchString) ||
                                         p.BackendTech.Contains(SearchString));
            }

            Products = await query.OrderByDescending(p => p.CreatedOn).ToListAsync();
        }
    }
}