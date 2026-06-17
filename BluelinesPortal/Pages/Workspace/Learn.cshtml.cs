using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Workspace
{
    [Authorize]
    public class LearnModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public LearnModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ProgramItem ActiveProgram { get; set; }
        public List<Module> Curriculum { get; set; }
        public Lesson CurrentLesson { get; set; }

        public bool IsEnrolled { get; set; }
        public bool HasAccessToCurrentLesson { get; set; }

        public async Task<IActionResult> OnGetAsync(int programId, int? lessonId)
        {
            var userId = _userManager.GetUserId(User);
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (profile == null) return RedirectToPage("/Student/ProfileSetup");

            // 1. Fetch the Program and its full Syllabus
            ActiveProgram = await _context.Programs.FirstOrDefaultAsync(p => p.Id == programId);
            if (ActiveProgram == null) return NotFound();

            Curriculum = await _context.Modules
                .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
                .Where(m => m.ProgramItemId == programId)
                .OrderBy(m => m.OrderIndex)
                .ToListAsync();

            // 2. Security Check: Is the student actually enrolled/paid?
            IsEnrolled = await _context.Applications.AnyAsync(a =>
                a.StudentProfileId == profile.Id &&
                a.ProgramItemId == programId &&
                a.Status == ApplicationStatus.Enrolled);

            // 3. Determine which lesson to display
            if (lessonId.HasValue)
            {
                CurrentLesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId.Value);
            }
            else if (Curriculum.Any() && Curriculum.First().Lessons.Any())
            {
                CurrentLesson = Curriculum.First().Lessons.First(); // Default to first lesson
            }

            // 4. Content Access Logic
            if (CurrentLesson != null)
            {
                HasAccessToCurrentLesson = IsEnrolled || CurrentLesson.IsFreePreview;

                // Scrub the premium content if they don't have access so it can't be scraped
                if (!HasAccessToCurrentLesson)
                {
                    CurrentLesson.VideoUrl = null;
                    CurrentLesson.Content = "Premium content locked.";
                }
            }

            return Page();
        }
    }
}