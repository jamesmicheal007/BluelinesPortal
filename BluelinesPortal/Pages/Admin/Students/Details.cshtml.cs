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

namespace BluelinesPortal.Pages.Admin.Students
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public StudentProfile Student { get; set; }
        public string Email { get; set; }

        public IList<StudentApplication> Applications { get; set; }
        public IList<PaymentRecord> Payments { get; set; }
        public IList<Submission> Submissions { get; set; }

        public decimal TotalPaid { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Get Profile & Email
            Student = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.Id == id);
            if (Student == null) return NotFound();

            var user = await _userManager.FindByIdAsync(Student.IdentityUserId);
            Email = user?.Email ?? "N/A";

            // 2. Get Applications
            Applications = await _context.Applications
                .Include(a => a.Program)
                .Where(a => a.StudentProfileId == id)
                .OrderByDescending(a => a.AppliedOn)
                .ToListAsync();

            // 3. Get Payments & Submissions linked to these applications
            var appIds = Applications.Select(a => a.Id).ToList();

            Payments = await _context.Payments
                .Include(p => p.Application.Program)
                .Where(p => appIds.Contains(p.StudentApplicationId) && p.PaymentStatus == "Success")
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            TotalPaid = Payments.Sum(p => p.AmountPaid);

            Submissions = await _context.Submissions
                .Include(s => s.Application.Program)
                .Where(s => appIds.Contains(s.StudentApplicationId))
                .OrderByDescending(s => s.SubmittedOn)
                .ToListAsync();

            return Page();
        }

        // --- QUESTPDF REPORT GENERATOR ---
        public async Task<IActionResult> OnGetDownloadReportAsync(int id)
        {
            await OnGetAsync(id); // Load the data using existing method

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("STUDENT DOSSIER").FontSize(24).Bold().FontColor("#0d6efd");
                        row.ConstantItem(150).AlignRight().Text($"Date: {DateTime.Now:dd MMM yyyy}");
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        // Profile Info
                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Row(row => {
                            row.RelativeItem().Column(c => {
                                c.Item().Text(Student.FullName).FontSize(16).Bold();
                                c.Item().Text($"Student ID: {Student.StudentId}").Bold().FontColor("#0d6efd");
                                c.Item().Text($"College: {Student.CollegeName}");
                            });
                            row.RelativeItem().AlignRight().Column(c => {
                                c.Item().Text($"Email: {Email}");
                                c.Item().Text($"Phone: {Student.PhoneNumber}");
                                c.Item().Text($"Total Revenue: Rs. {TotalPaid:N2}").Bold().FontColor(Colors.Green.Darken1);
                            });
                        });

                        // Applications Section
                        col.Item().PaddingTop(20).Text("PROGRAM ENROLLMENTS").FontSize(12).Bold().Underline();
                        foreach (var app in Applications)
                        {
                            col.Item().PaddingTop(5).Text($"• {app.Program.Title} (Status: {app.Status})");
                        }

                        // Financials Section
                        col.Item().PaddingTop(20).Text("FINANCIAL HISTORY").FontSize(12).Bold().Underline();
                        if (Payments.Any())
                        {
                            foreach (var pay in Payments)
                            {
                                col.Item().PaddingTop(5).Text($"• {pay.PaymentDate:dd MMM yy} | {pay.Application.Program.Title} | Rs. {pay.AmountPaid:N2} | Ref: {pay.PaymentGatewayReference}");
                            }
                        }
                        else { col.Item().Text("No payment history."); }

                        // Submissions Section
                        col.Item().PaddingTop(20).Text("ACADEMIC SUBMISSIONS").FontSize(12).Bold().Underline();
                        if (Submissions.Any())
                        {
                            foreach (var sub in Submissions)
                            {
                                col.Item().PaddingTop(5).Text($"• [{sub.ReviewStatus}] {sub.SubmissionTitle} | Submitted: {sub.SubmittedOn:dd MMM yy}");
                            }
                        }
                        else { col.Item().Text("No academic submissions."); }
                    });

                    page.Footer().AlignCenter().Text($"Bluelines Tech Solutions - Official Internal Record");
                });
            });

            return File(document.GeneratePdf(), "application/pdf", $"Dossier_{Student.StudentId}.pdf");
        }
    }
}