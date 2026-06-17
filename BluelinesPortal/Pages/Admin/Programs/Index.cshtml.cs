using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Programs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProgramItem> Programs { get; set; }

        // Filter & Search Properties
        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public ProgramType? TypeFilter { get; set; }
        [BindProperty(SupportsGet = true)] public string SortOrder { get; set; }

        // Pagination Properties
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            IQueryable<ProgramItem> programsIQ = _context.Programs.AsQueryable();

            // 1. Search
            if (!string.IsNullOrEmpty(SearchString))
            {
                programsIQ = programsIQ.Where(p => p.Title.Contains(SearchString) || p.ShortDescription.Contains(SearchString));
            }

            // 2. Filter
            if (TypeFilter.HasValue)
            {
                programsIQ = programsIQ.Where(p => p.Type == TypeFilter.Value);
            }

            // 3. Sort
            ViewData["TitleSort"] = String.IsNullOrEmpty(SortOrder) ? "title_desc" : "";
            ViewData["FeeSort"] = SortOrder == "Fee" ? "fee_desc" : "Fee";

            programsIQ = SortOrder switch
            {
                "title_desc" => programsIQ.OrderByDescending(p => p.Title),
                "Fee" => programsIQ.OrderBy(p => p.BaseFee),
                "fee_desc" => programsIQ.OrderByDescending(p => p.BaseFee),
                _ => programsIQ.OrderBy(p => p.Title),
            };

            // 4. Paginate
            var count = await programsIQ.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            Programs = await programsIQ
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}