using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace BluelinesPortal.Pages.Admin.Students
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // View Model specifically for the grid
        public class StudentListDto
        {
            public int ProfileId { get; set; }
            public string StudentId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string CollegeName { get; set; }
            public string Phone { get; set; }
            public int EnrolledPrograms { get; set; }
        }

        public IList<StudentListDto> Students { get; set; }

        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public string SortOrder { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 15;
        public int TotalStudents { get; set; }

        public async Task OnGetAsync()
        {
            TotalStudents = await _context.StudentProfiles.CountAsync();

            // Securely join Profiles with Identity Users to get Email
            var query = from p in _context.StudentProfiles
                        join u in _context.Users on p.IdentityUserId equals u.Id
                        select new StudentListDto
                        {
                            ProfileId = p.Id,
                            StudentId = p.StudentId ?? "PENDING",
                            FullName = p.FullName,
                            Email = u.Email,
                            CollegeName = p.CollegeName,
                            Phone = p.PhoneNumber,
                            EnrolledPrograms = p.Applications.Count(a => a.Status == ApplicationStatus.Enrolled)
                        };

            // 1. Search Logic
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(s =>
                    s.FullName.Contains(SearchString) ||
                    s.StudentId.Contains(SearchString) ||
                    s.Email.Contains(SearchString) ||
                    s.CollegeName.Contains(SearchString));
            }

            // 2. Sorting Logic
            ViewData["NameSort"] = String.IsNullOrEmpty(SortOrder) ? "name_desc" : "";
            ViewData["IdSort"] = SortOrder == "ID" ? "id_desc" : "ID";
            ViewData["ProgramSort"] = SortOrder == "Programs" ? "prog_desc" : "Programs";

            query = SortOrder switch
            {
                "name_desc" => query.OrderByDescending(s => s.FullName),
                "ID" => query.OrderBy(s => s.StudentId),
                "id_desc" => query.OrderByDescending(s => s.StudentId),
                "Programs" => query.OrderBy(s => s.EnrolledPrograms),
                "prog_desc" => query.OrderByDescending(s => s.EnrolledPrograms),
                _ => query.OrderBy(s => s.FullName), // Default Sort
            };

            // 3. Pagination Logic
            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            Students = await query.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToListAsync();
        }

        // --- CSV EXPORT HANDLER ---
        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            var query = await (from p in _context.StudentProfiles
                               join u in _context.Users on p.IdentityUserId equals u.Id
                               orderby p.StudentId
                               select new
                               {
                                   p.StudentId,
                                   p.FullName,
                                   u.Email,
                                   p.PhoneNumber,
                                   p.CollegeName,
                                   Enrolled = p.Applications.Count(a => a.Status == ApplicationStatus.Enrolled)
                               }).ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Student ID,Full Name,Email,Phone,College,Enrolled Programs");

            foreach (var s in query)
            {
                // Escape quotes and commas for safe CSV formatting
                csv.AppendLine($"{s.StudentId},\"{s.FullName}\",\"{s.Email}\",\"{s.PhoneNumber}\",\"{s.CollegeName}\",{s.Enrolled}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"Student_Directory_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}