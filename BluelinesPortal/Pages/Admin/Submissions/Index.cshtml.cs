using BluelinesPortal.Data;
using BluelinesPortal.Models;
using BluelinesPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // <-- Required for UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Submissions
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager; // <-- Added

        public IndexModel(ApplicationDbContext context, IEmailService emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        public IList<Submission> StudentSubmissions { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; } = "Pending";

        public async Task OnGetAsync()
        {
            var query = _context.Submissions
                .Include(s => s.Application).ThenInclude(a => a.Student)
                .Include(s => s.Application).ThenInclude(a => a.Program)
                .AsQueryable();

            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "All")
            {
                query = query.Where(s => s.ReviewStatus == StatusFilter);
            }

            StudentSubmissions = await query.OrderByDescending(s => s.SubmittedOn).ToListAsync();
        }

        public async Task<IActionResult> OnPostGradeAsync(int submissionId, string reviewStatus, string mentorFeedback)
        {
            var submission = await _context.Submissions
                .Include(s => s.Application).ThenInclude(a => a.Student)
                .Include(s => s.Application).ThenInclude(a => a.Program)
                .FirstOrDefaultAsync(s => s.Id == submissionId);

            if (submission != null)
            {
                submission.ReviewStatus = reviewStatus;
                submission.MentorFeedback = mentorFeedback;
                submission.ReviewedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Submission for {submission.Application.Program.Title} successfully marked as {reviewStatus}.";

                if (reviewStatus == "Approved")
                {
                    // === SAFELY FETCH EMAIL FROM ASP.NET IDENTITY ===
                    var identityUser = await _userManager.FindByIdAsync(submission.Application.Student.IdentityUserId);
                    var secureEmail = identityUser?.Email;

                    if (!string.IsNullOrEmpty(secureEmail))
                    {
                        string subject = "🎉 Project Approved! Download Your Certificate";

                        string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eaeaeb; border-radius: 8px; overflow: hidden;'>
                                <div style='background-color: #0d6efd; padding: 30px; text-align: center; color: white;'>
                                    <h1 style='margin: 0;'>Congratulations!</h1>
                                </div>
                                <div style='padding: 30px; color: #333;'>
                                    <p style='font-size: 16px;'>Hi <strong>{submission.Application.Student.FullName}</strong>,</p>
                                    <p style='font-size: 16px; line-height: 1.5;'>Your final submission for the <strong>{submission.Application.Program.Title}</strong> program has been officially reviewed and approved by your mentor.</p>
                                    
                                    <div style='background-color: #f8f9fa; border-left: 4px solid #198754; padding: 15px; margin: 25px 0;'>
                                        <p style='margin: 0; font-size: 14px; color: #555;'><strong>Mentor Feedback:</strong></p>
                                        <p style='margin: 10px 0 0 0; font-style: italic;'>&quot;{mentorFeedback}&quot;</p>
                                    </div>

                                    <p style='font-size: 16px; line-height: 1.5;'>Your cryptographically verifiable PDF Certificate of Completion is now available. Log in to your portal workspace to download it and add it to your LinkedIn profile.</p>
                                    
                                    <div style='text-align: center; margin-top: 40px;'>
                                        <a href='https://yourdomain.com/Workspace' style='background-color: #0d6efd; color: white; padding: 14px 28px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>Access Your Workspace</a>
                                    </div>
                                </div>
                            </div>";

                        await _emailService.SendEmailAsync(secureEmail, submission.Application.Student.FullName, subject, htmlMessage);
                    }
                }
            }

            return RedirectToPage(new { StatusFilter = StatusFilter });
        }
    }
}