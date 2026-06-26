using BluelinesPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BluelinesPortal.Pages.Admin.Certificates
{
    [Authorize(Roles = "Admin")]
    public class ManageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ManageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty] public int ApplicationId { get; set; }
        [BindProperty] public int ProfileId { get; set; }

        // Editable Certificate Fields
        [BindProperty] public string StudentName { get; set; }
        [BindProperty] public string StudentId { get; set; }
        [BindProperty] public string Gender { get; set; }
        [BindProperty] public string DegreeProgram { get; set; }
        [BindProperty] public string CollegeName { get; set; }
        [BindProperty] public string ProgramTitle { get; set; }

        [BindProperty] public DateTime StartDate { get; set; }
        [BindProperty] public DateTime EndDate { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var application = await _context.Applications
                .Include(a => a.Program)
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null) return NotFound();

            ApplicationId = application.Id;
            ProfileId = application.StudentProfileId;

            // Pre-fill the form with the current database values
            StudentName = application.Student.FullName;
            StudentId = application.Student.StudentId ?? "";
            Gender = application.Student.Gender ?? "Male";
            DegreeProgram = application.Student.DegreeProgram ?? "COMPUTER SCIENCE";
            CollegeName = application.Student.CollegeName ?? "UNIVERSITY";
            ProgramTitle = application.Program.Title;

            StartDate = application.AppliedOn;
            EndDate = application.AppliedOn.AddDays(application.Program.DurationInDays);

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateDatabaseAsync()
        {
            var profile = await _context.StudentProfiles.FindAsync(ProfileId);
            if (profile != null)
            {
                // Permanently fix typos in the student's profile
                profile.FullName = StudentName;
                profile.Gender = Gender;
                profile.DegreeProgram = DegreeProgram;
                profile.CollegeName = CollegeName;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Student profile has been permanently updated with these corrections.";
            }
            return RedirectToPage(new { id = ApplicationId });
        }

        public IActionResult OnPostDownloadPdf()
        {
            // --- PRONOUN & PREFIX LOGIC ---
            string prefix = "Mr./Ms. ";
            string pronounSubject = "They";
            string pronounObject = "them";
            string pronounPossessive = "their";

            var checkGender = Gender?.ToLower() ?? "";
            if (checkGender == "male")
            {
                prefix = "Mr. ";
                pronounSubject = "He";
                pronounObject = "him";
                pronounPossessive = "his";
            }
            else if (checkGender == "female")
            {
                prefix = "Ms. ";
                pronounSubject = "She";
                pronounObject = "her";
                pronounPossessive = "her";
            }

            string displayStudentId = string.IsNullOrEmpty(StudentId) ? "" : $"({StudentId})";

            // --- GENERATE PDF FROM FORM DATA (Not Database) ---
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    page.Background(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(14).FontFamily(Fonts.Arial).LineHeight(1.5f));

                    page.Content().Padding(30).Border(4).BorderColor("#0d6efd").Padding(4).Border(1).BorderColor("#0d6efd").Padding(50)
                        .Column(col =>
                        {
                            col.Item().Text($"{DateTime.Now:MMMM dd, yyyy}").FontSize(12).SemiBold();
                            col.Item().PaddingTop(20).AlignCenter().Text("CERTIFICATE OF COMPLETION").FontSize(32).Bold().FontColor("#0d6efd");

                            col.Item().PaddingTop(40).Text(text =>
                            {
                                text.Span("To whom it may concern,\n\n").SemiBold();
                                text.Span("\tThis is to certify that  ");
                                text.Span($"{prefix} {StudentName.ToUpper()}  {displayStudentId}  ").Bold();
                                text.Span("from  ");
                                text.Span($"DEPARTMENT OF {DegreeProgram.ToUpper()},  {CollegeName.ToUpper()}  ").Bold();
                                text.Span("has successfully completed an internship program at Bluelines Tech Solutions, Kovilpatti as a ");
                                text.Span($"{ProgramTitle.ToUpper()}").Bold();
                                text.Span(".");
                            });

                            col.Item().PaddingTop(20).Text(text =>
                            {
                                text.Span($"\t{pronounSubject} was part of the program from ");
                                text.Span($"{StartDate:dd-MM-yyyy}").Bold();
                                text.Span(" to ");
                                text.Span($"{EndDate:dd-MM-yyyy}").Bold();
                                text.Span($". During {pronounPossessive} tenure as an intern, {pronounSubject.ToLower()} demonstrated enthusiasm, leadership, self-discipline, and self-motivation.\n\n");
                                text.Span($"\tWe were fortunate to have {pronounObject} as one of our interns, and we wish {pronounObject} all the best in {pronounPossessive} future endeavors.");
                            });

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

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Official_Certificate_{StudentName.Replace(" ", "_")}.pdf");
        }
    }
}