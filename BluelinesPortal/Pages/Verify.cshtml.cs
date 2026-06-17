using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages
{
    public class VerifyModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VerifyModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Certificate VerifiedCertificate { get; set; }
        public bool SearchAttempted { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                SearchAttempted = true;

                VerifiedCertificate = await _context.Certificates
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Student)
                    .Include(c => c.Application)
                        .ThenInclude(a => a.Program)
                    .FirstOrDefaultAsync(c => c.VerificationId == id.Trim().ToUpper());
            }

            return Page();
        }
    }
}