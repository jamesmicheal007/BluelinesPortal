using BluelinesPortal.Data;
using BluelinesPortal.Models;
using BluelinesPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // <-- Required
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Applications
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

        public IList<StudentApplication> Applications { get; set; }

        [BindProperty(SupportsGet = true)]
        public ApplicationStatus? StatusFilter { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .AsQueryable();

            if (StatusFilter.HasValue)
            {
                query = query.Where(a => a.Status == StatusFilter.Value);
            }
            else
            {
                query = query.OrderBy(a => a.Status == ApplicationStatus.Pending ? 0 :
                                           a.Status == ApplicationStatus.UnderReview ? 1 : 2);
            }

            Applications = await query.OrderByDescending(a => a.AppliedOn).ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, ApplicationStatus newStatus)
        {
            var application = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application != null)
            {
                application.Status = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Application #{id} successfully moved to {newStatus}.";

                if (newStatus == ApplicationStatus.Approved)
                {
                    // === SAFELY FETCH EMAIL FROM ASP.NET IDENTITY ===
                    var identityUser = await _userManager.FindByIdAsync(application.Student.IdentityUserId);
                    var secureEmail = identityUser?.Email;

                    if (!string.IsNullOrEmpty(secureEmail))
                    {
                        string subject = $"Action Required: Official Offer for {application.Program.Title}";

                        string htmlMessage = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eaeaeb; border-radius: 8px; overflow: hidden;'>
                                <div style='background-color: #0d6efd; padding: 30px; text-align: center; color: white;'>
                                    <h1 style='margin: 0;'>Application Approved!</h1>
                                </div>
                                <div style='padding: 30px; color: #333;'>
                                    <p style='font-size: 16px;'>Dear <strong>{application.Student.FullName}</strong>,</p>
                                    <p style='font-size: 16px; line-height: 1.5;'>Congratulations! Your application for the <strong>{application.Program.Title}</strong> program has been reviewed and officially approved.</p>
                                    
                                    <p style='font-size: 16px; line-height: 1.5;'>To secure your spot and gain immediate access to your workspace and learning materials, please log in to the portal to view and accept your offer.</p>
                                    
                                    <div style='text-align: center; margin-top: 40px; margin-bottom: 20px;'>
                                        <a href='https://yourdomain.com/Dashboard' style='background-color: #0d6efd; color: white; padding: 14px 28px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>View My Offer</a>
                                    </div>
                                </div>
                            </div>";
                        try
                        {
                            await _emailService.SendEmailAsync(secureEmail, application.Student.FullName, subject, htmlMessage);
                        }
                        catch (Exception ex)
                        {
                            // Log the error (optional), but allow the page to continue executing
                            Console.WriteLine($"Email failed to send to {secureEmail}: {ex.Message}");
                            TempData["WarningMessage"] = "Action saved, but the notification email failed to send.";
                        }
                    }
                }
            }
            return RedirectToPage(new { StatusFilter = StatusFilter });
        }
    }
}