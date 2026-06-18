using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Finance
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<PaymentRecord> RecentPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }

        // Filter, Sort, and Paginate properties
        [BindProperty(SupportsGet = true)] public string SearchString { get; set; }
        [BindProperty(SupportsGet = true)] public string SortOrder { get; set; }
        [BindProperty(SupportsGet = true)] public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;

        public async Task OnGetAsync()
        {
            // 1. Calculate Top-Level Metrics (Always reflects the whole database)
            TotalRevenue = await _context.Payments
                .Where(p => p.PaymentStatus == "Success")
                .SumAsync(p => p.AmountPaid);

            TotalTransactions = await _context.Payments
                .CountAsync(p => p.PaymentStatus == "Success");

            // 2. Base Query
            var query = _context.Payments
                .Include(p => p.Application).ThenInclude(a => a.Student)
                .Include(p => p.Application).ThenInclude(a => a.Program)
                .AsQueryable();

            // 3. Search Logic
            if (!string.IsNullOrEmpty(SearchString))
            {
                query = query.Where(p =>
                    p.Application.Student.FullName.Contains(SearchString) ||
                    p.PaymentGatewayReference.Contains(SearchString) ||
                    p.Application.Program.Title.Contains(SearchString));
            }

            // 4. Sorting Logic
            ViewData["DateSort"] = String.IsNullOrEmpty(SortOrder) ? "date_asc" : "";
            ViewData["NameSort"] = SortOrder == "Name" ? "name_desc" : "Name";
            ViewData["AmountSort"] = SortOrder == "Amount" ? "amount_desc" : "Amount";

            query = SortOrder switch
            {
                "date_asc" => query.OrderBy(p => p.PaymentDate),
                "Name" => query.OrderBy(p => p.Application.Student.FullName),
                "name_desc" => query.OrderByDescending(p => p.Application.Student.FullName),
                "Amount" => query.OrderBy(p => p.AmountPaid),
                "amount_desc" => query.OrderByDescending(p => p.AmountPaid),
                _ => query.OrderByDescending(p => p.PaymentDate), // Default is Newest First
            };

            // 5. Pagination Logic
            var count = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(count / (double)PageSize);

            // Safety check for empty searches
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            RecentPayments = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
        public async Task<IActionResult> OnPostVerifyPaymentAsync(int paymentId, string action)
        {
            var payment = await _context.Payments
                .Include(p => p.Application)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment != null && payment.PaymentStatus == "PendingVerification")
            {
                if (action == "Approve")
                {
                    payment.PaymentStatus = "Success";

                    // Auto-enroll the student if this payment clears their balance or moves them from Approved
                    if (payment.Application.Status == ApplicationStatus.Approved)
                    {
                        payment.Application.Status = ApplicationStatus.Enrolled;
                    }

                    TempData["SuccessMessage"] = $"Payment of ₹{payment.AmountPaid} verified. Student enrolled.";
                }
                else if (action == "Reject")
                {
                    payment.PaymentStatus = "Rejected";
                    TempData["SuccessMessage"] = "Payment proof rejected.";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { SearchString = SearchString, SortOrder = SortOrder, CurrentPage = CurrentPage });
        }
    }
}