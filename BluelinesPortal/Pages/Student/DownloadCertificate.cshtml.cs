using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BluelinesPortal.Pages.Student
{
    [Authorize]
    public class DownloadCertificateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DownloadCertificateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            // 1. Verify the student is enrolled in this program
            var application = await _context.Applications
                .Include(a => a.Student)
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentProfileId == profile.Id && a.Status == ApplicationStatus.Enrolled);

            if (application == null) return NotFound();

            // 2. Verify they actually have an 'Approved' submission from the mentor
            var hasApprovedWork = await _context.Submissions
                .AnyAsync(s => s.StudentApplicationId == id && s.ReviewStatus == "Approved");

            if (!hasApprovedWork)
            {
                TempData["ErrorMessage"] = "You must have an approved milestone to claim your certificate.";
                return RedirectToPage("/Workspace/ProjectRoom", new { id = id });
            }

            // 3. Create or fetch the Certificate Database Record
            var certificate = await _context.Certificates.FirstOrDefaultAsync(c => c.StudentApplicationId == id);

            if (certificate == null)
            {
                certificate = new Certificate
                {
                    VerificationId = "BLT-" + DateTime.Now.Year + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                    StudentApplicationId = id,
                    IssuedDate = DateTime.UtcNow
                };
                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();
            }

            // 4. Draw the PDF using QuestPDF
            var verificationUrl = $"{Request.Scheme}://{Request.Host}/Verify?id={certificate.VerificationId}";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    // Drawing the certificate border and layout
                    page.Content().Border(8).BorderColor("#0d6efd").Padding(1.5f, Unit.Inch).Column(col =>
                    {
                        col.Item().AlignCenter().Text("BLUELINES TECH SOLUTIONS").FontSize(28).Bold().FontColor("#0d6efd").LetterSpacing(0.05f);

                        // FIX: Move PaddingBottom(20) before .Text()
                        col.Item().PaddingBottom(20).AlignCenter().Text("CERTIFICATE OF COMPLETION").FontSize(36).Black();

                        col.Item().AlignCenter().Text("This is proudly presented to").FontSize(16).Italic().FontColor(Colors.Grey.Darken2);

                        // FIX: Move PaddingVertical(10) before .Text()
                        col.Item().PaddingVertical(10).AlignCenter().Text(application.Student.FullName).FontSize(38).Bold().FontColor(Colors.Grey.Darken4);

                        col.Item().AlignCenter().Text("for successfully completing the rigorous technical requirements of").FontSize(16).Italic();

                        // FIX: Move PaddingTop(10) before .Text()
                        col.Item().PaddingTop(10).AlignCenter().Text(application.Program.Title).FontSize(24).Bold().FontColor("#0d6efd");

                        // Signatures and Verification Footer
                        col.Item().PaddingTop(50).Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("Michael James").FontSize(16).Bold();
                                c.Item().Text("Director, Bluelines Tech");
                                c.Item().Text("Kovilpatti, Tamil Nadu").FontSize(10).FontColor(Colors.Grey.Medium);
                            });

                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text($"Issue Date: {certificate.IssuedDate:dd MMM yyyy}");
                                c.Item().Text($"Verify at: {verificationUrl}").FontSize(10).FontColor(Colors.Blue.Medium).Underline();
                                c.Item().Text($"ID: {certificate.VerificationId}").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
                });
            });

            // Return the drawn PDF as a file download
            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"{application.Student.FullName.Replace(" ", "_")}_Certificate.pdf");
        }
    }
}