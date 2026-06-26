using BluelinesPortal.Data;
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

            // Fetch the specific application and ensure the student is enrolled
            var application = await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id && a.Student.IdentityUserId == userId && a.Status == Models.ApplicationStatus.Enrolled);

            if (application == null)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            // Verify they have an Approved submission before generating the certificate
            var hasApprovedProject = await _context.Submissions
                .AnyAsync(s => s.StudentApplicationId == id && s.ReviewStatus == "Approved");

            if (!hasApprovedProject)
            {
                TempData["WarningMessage"] = "You must have an approved submission to download the certificate.";
                return RedirectToPage("/Workspace/Index", new { id = application.Id });
            }

            // --- DATE LOGIC ---
            var startDate = application.AppliedOn;
            var endDate = startDate.AddDays(application.Program.DurationInDays);

            // --- PRONOUN & PREFIX LOGIC ---
            string prefix = "Mr./Ms. ";
            string pronounSubject = "They";
            string pronounObject = "them";
            string pronounPossessive = "their";

            var gender = application.Student.Gender?.ToLower();
            if (gender == "male")
            {
                prefix = "Mr. ";
                pronounSubject = "He";
                pronounObject = "him";
                pronounPossessive = "his";
            }
            else if (gender == "female")
            {
                prefix = "Ms. ";
                pronounSubject = "She";
                pronounObject = "her";
                pronounPossessive = "her";
            }

            // --- FALLBACK LOGIC FOR NULLS ---
            string degree = string.IsNullOrEmpty(application.Student.DegreeProgram) ? "COMPUTER SCIENCE" : application.Student.DegreeProgram;
            string college = string.IsNullOrEmpty(application.Student.CollegeName) ? "UNIVERSITY" : application.Student.CollegeName;
            string studentId = string.IsNullOrEmpty(application.Student.StudentId) ? "" : $"({application.Student.StudentId})";

            // ==========================================
            // 💡 QUEST PDF: EXACT MATCH TO YOUR WORD DOC
            // ==========================================
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14).FontFamily(Fonts.Arial).LineHeight(1.5f));

                    // Outer padding and Double Border
                    page.Content()
                        .Padding(30)
                        .Border(4).BorderColor("#0d6efd") // Thick Blue Outer Border
                        .Padding(4)
                        .Border(1).BorderColor("#0d6efd") // Thin Blue Inner Border
                        .Padding(50)
                        .Column(col =>
                        {
                            // 1. Date (Top Left)
                            col.Item().Text($"{DateTime.Now:MMMM dd, yyyy}").FontSize(12).SemiBold();

                            // 2. Title (Center)
                            col.Item().PaddingTop(20).AlignCenter()
                               .Text("CERTIFICATE OF COMPLETION")
                               .FontSize(32).Bold().FontColor("#0d6efd");

                            // 3. Body Text (Matching your exact wording)
                            col.Item().PaddingTop(40).Text(text =>
                            {
                                text.Span("To whom it may concern,\n\n").SemiBold();

                                text.Span("\tThis is to certify that  ");
                                text.Span($"{prefix} {application.Student.FullName.ToUpper()}  {studentId}  ").Bold();
                                text.Span("from  ");
                                text.Span($"DEPARTMENT OF {degree.ToUpper()},  {college.ToUpper()}  ").Bold();
                                text.Span("has successfully completed an internship program at Bluelines Tech Solutions, Kovilpatti as a ");
                                text.Span($"{application.Program.Title.ToUpper()}").Bold();
                                text.Span(".");
                            });

                            col.Item().PaddingTop(20).Text(text =>
                            {
                                text.Span($"\t{pronounSubject} was part of the program from ");
                                text.Span($"{startDate:dd-MM-yyyy}").Bold();
                                text.Span(" to ");
                                text.Span($"{endDate:dd-MM-yyyy}").Bold();
                                text.Span($". During {pronounPossessive} tenure as an intern, {pronounSubject.ToLower()} demonstrated enthusiasm, leadership, self-discipline, and self-motivation.\n\n");

                                text.Span($"\tWe were fortunate to have {pronounObject} as one of our interns, and we wish {pronounObject} all the best in {pronounPossessive} future endeavors.");
                            });

                            // 4. Signature Block (Bottom Left)
                            col.Item().PaddingTop(40).Column(sig =>
                            {
                                sig.Item().Text("Sincerely,").SemiBold();
                                sig.Item().PaddingTop(30).Text("MICHAEL JAMES S").Bold();
                                sig.Item().Text("(Project Manager)");
                                sig.Item().Text("+91 9789175161");
                            });
                        });
                });
            });

            // Generate and return the PDF file
            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Bluelines_Certificate_{application.Student.FullName.Replace(" ", "_")}.pdf");
        }
    }
}