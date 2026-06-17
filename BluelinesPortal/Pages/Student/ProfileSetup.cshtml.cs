using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BluelinesPortal.Pages.Student
{
    [Authorize]
    public class ProfileSetupModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileSetupModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public StudentProfile Profile { get; set; }

        // CRITICAL FIX: The return type must be Task<IActionResult>, not Task
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            var existingProfile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (existingProfile != null)
            {
                return RedirectToPage("/Dashboard/Index");
            }

            Profile = new StudentProfile();

            var nameClaim = User.FindFirstValue(ClaimTypes.Name);
            if (!string.IsNullOrEmpty(nameClaim))
            {
                Profile.FullName = nameClaim;
            }

            return Page();
        }

        /// CRITICAL FIX: The return type must be Task<IActionResult>, not Task
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            var userId = _userManager.GetUserId(User);
            Profile.IdentityUserId = userId;

            // ==========================================
            // 💡 GENERATE THE UNIQUE STUDENT ID HERE
            // ==========================================
            // This counts how many students already exist and adds 1 to create the next number
            int studentCount = await _context.StudentProfiles.CountAsync();
            Profile.StudentId = $"BLT{DateTime.Now:yy}{(studentCount + 1):D3}";

            _context.StudentProfiles.Add(Profile);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Dashboard/Index");
        }
    }
}