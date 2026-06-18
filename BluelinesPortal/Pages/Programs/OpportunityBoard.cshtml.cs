using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Programs
{
    [Authorize]
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
            // Fetch only programs marked as active by the Admin
            ActivePrograms = await _context.Programs
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }
    }
}