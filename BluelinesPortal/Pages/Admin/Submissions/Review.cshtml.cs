using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Submissions
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
        public Submission SubmissionRecord { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            SubmissionRecord = await _context.Submissions
                .Include(s => s.Application)
                    .ThenInclude(a => a.Student)
                .Include(s => s.Application)
                    .ThenInclude(a => a.Program)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SubmissionRecord == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var recordToUpdate = await _context.Submissions.FindAsync(SubmissionRecord.Id);

            if (recordToUpdate == null) return NotFound();

            // Update only the fields the mentor is allowed to touch
            recordToUpdate.ReviewStatus = SubmissionRecord.ReviewStatus;

            // Prevent null crashes if the mentor leaves the feedback box completely empty
            recordToUpdate.MentorFeedback = SubmissionRecord.MentorFeedback ?? "";

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}