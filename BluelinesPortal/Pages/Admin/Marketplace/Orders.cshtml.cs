using BluelinesPortal.Data;
using BluelinesPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Pages.Admin.Marketplace
{
    [Authorize(Roles = "Admin")]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public OrdersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<ProductOrder> AllOrders { get; set; }

        public async Task OnGetAsync()
        {
            // Fetch all orders with their related Student and Product data
            AllOrders = await _context.ProductOrders
                .Include(o => o.Student)
                .Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string newStatus)
        {
            var order = await _context.ProductOrders.FindAsync(orderId);

            if (order != null)
            {
                order.OrderStatus = newStatus;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Order #{orderId} has been successfully marked as {newStatus}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyBalanceAsync(int orderId)
        {
            var order = await _context.ProductOrders.FindAsync(orderId);
            if (order != null && order.BalanceStatus == "PendingVerification")
            {
                // Add the balance to the total amount paid
                order.AmountPaid += order.BalanceDue;

                // Clear the debt and mark as paid
                order.BalanceDue = 0;
                order.BalanceStatus = "Paid";

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Balance for Order #{orderId} verified. Premium files are now unlocked for the student!";
            }
            return RedirectToPage();
        }
    }
}