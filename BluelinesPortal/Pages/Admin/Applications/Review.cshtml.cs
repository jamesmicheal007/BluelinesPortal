using BluelinesPortal.Data;
using BluelinesPortal.Models;
using BluelinesPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Applications
{
    [Authorize(Roles = "Admin")]
    public class ReviewModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewModel(ApplicationDbContext context, IEmailService emailService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        public StudentApplication ApplicationRecord { get; set; }
        public string StudentEmail { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ApplicationRecord = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (ApplicationRecord == null) return NotFound();

            var identityUser = await _userManager.FindByIdAsync(ApplicationRecord.Student.IdentityUserId);
            StudentEmail = identityUser?.Email ?? "No Email Found";

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int id, ApplicationStatus newStatus, string adminNotes)
        {
            var application = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application != null)
            {
                application.Status = newStatus;
                application.AdminNotes = adminNotes; // Save your internal notes

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Application #{id} successfully moved to {newStatus}.";

                // === TRIGGER THE ACCEPTANCE EMAIL ===
                if (newStatus == ApplicationStatus.Approved)
                {
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
                                    <p style='font-size: 16px; line-height: 1.5;'>To secure your spot and gain immediate access to your workspace, please log in to the portal to view and accept your offer.</p>
                                    <div style='text-align: center; margin-top: 40px; margin-bottom: 20px;'>
                                        <a href='https://yourdomain.com/Dashboard' style='background-color: #0d6efd; color: white; padding: 14px 28px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>View My Offer</a>
                                    </div>
                                </div>
                            </div>";

                        // Wrap in try-catch to prevent a crash if the SMTP server is busy
                        try
                        {
                            await _emailService.SendEmailAsync(secureEmail, application.Student.FullName, subject, htmlMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Email Error: {ex.Message}");
                        }
                    }
                }
            }
            return RedirectToPage("./Index");
        }
    }
}