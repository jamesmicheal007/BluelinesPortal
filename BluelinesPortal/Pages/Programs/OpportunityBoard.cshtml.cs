using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Programs
{
    // Public Page - No [Authorize] attribute
    public class OpportunityBoardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OpportunityBoardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProgramItem> ActivePrograms { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch only active programs, ordered newest first
            ActivePrograms = await _context.Programs
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }
    }
}