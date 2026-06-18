using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Programs
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ProgramItem DeleteProgram { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeleteProgram = await _context.Programs.FirstOrDefaultAsync(m => m.Id == id);

            if (DeleteProgram == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            DeleteProgram = await _context.Programs.FindAsync(id);

            if (DeleteProgram != null)
            {
                // SOFT DELETE: Never physically remove a program with active students
                DeleteProgram.IsActive = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}