using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Applications
{
    [Authorize]
    public class ReviewModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReviewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudentApplication ApplicationRecord { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            ApplicationRecord = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ApplicationRecord == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Fetch the existing record to prevent overwriting everything
            var applicationToUpdate = await _context.Applications.FindAsync(ApplicationRecord.Id);

            if (applicationToUpdate == null) return NotFound();

            // Only update the Status and Admin Notes
            applicationToUpdate.Status = ApplicationRecord.Status;
            applicationToUpdate.AdminNotes = ApplicationRecord.AdminNotes;

            await _context.SaveChangesAsync();

            // Note: In the future, if Status == Approved, we would trigger an email here.

            return RedirectToPage("./Index");
        }
    }
}