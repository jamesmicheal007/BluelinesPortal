using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Marketplace
{
    [AllowAnonymous]
    public class ProjectsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProjectsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<DigitalProduct> Products { get; set; }
        public IList<ProductCategory> Categories { get; set; }

        // Filter Properties
        [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }

        // Pagination Properties
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 9; // Show 9 projects per page
        public int TotalProjects { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch all active categories for the Sidebar Filter
            Categories = await _context.ProductCategories
                .Where(c => c.IsActive)
                .AsNoTracking()
                .ToListAsync();

            // Start building the query
            var query = _context.DigitalProducts
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsNoTracking();

            // 1. Apply Category Filter
            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            // 2. Apply Text Search (Searches Title, Tech Stack, and Category Name)
            if (!string.IsNullOrEmpty(SearchString))
            {
                var lowerSearch = SearchString.ToLower();
                query = query.Where(p =>
                    p.Title.ToLower().Contains(lowerSearch) ||
                    p.FrontendTech.ToLower().Contains(lowerSearch) ||
                    p.BackendTech.ToLower().Contains(lowerSearch) ||
                    p.Category.Name.ToLower().Contains(lowerSearch));
            }

            // 3. Calculate Pagination Math
            TotalProjects = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalProjects / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // 4. Fetch only the data for the current page
            Products = await query
                .OrderByDescending(p => p.CreatedOn)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}