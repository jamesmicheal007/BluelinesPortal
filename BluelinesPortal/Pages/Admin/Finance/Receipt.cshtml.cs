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

namespace BluelinesPortal.Pages.Admin.Finance
{
    [Authorize(Roles = "Admin")]
    public class ReceiptModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReceiptModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public PaymentRecord BillDetails { get; set; }
        public string StudentEmail { get; set; }

        // NEW FINANCIAL PROPERTIES
        public decimal TotalPaidSoFar { get; set; }
        public decimal RemainingBalance { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            BillDetails = await _context.Payments
                .Include(p => p.Application).ThenInclude(a => a.Student)
                .Include(p => p.Application).ThenInclude(a => a.Program)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (BillDetails == null) return NotFound();

            var identityUser = await _userManager.FindByIdAsync(BillDetails.Application.Student.IdentityUserId);
            StudentEmail = identityUser?.Email ?? "No Email on Record";

            // Calculate Partial Payment Logic
            TotalPaidSoFar = await _context.Payments
                .Where(p => p.StudentApplicationId == BillDetails.StudentApplicationId && p.PaymentStatus == "Success")
                .SumAsync(p => p.AmountPaid);

            RemainingBalance = BillDetails.Application.Program.BaseFee - TotalPaidSoFar;
            if (RemainingBalance < 0) RemainingBalance = 0;

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadPdfAsync(int id)
        {
            var bill = await _context.Payments
                .Include(p => p.Application).ThenInclude(a => a.Student)
                .Include(p => p.Application).ThenInclude(a => a.Program)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (bill == null) return NotFound();

            var identityUser = await _userManager.FindByIdAsync(bill.Application.Student.IdentityUserId);
            var secureEmail = identityUser?.Email ?? "No Email on Record";

            // Calculate Partial Payment Logic for PDF
            var totalPaidSoFar = await _context.Payments
                .Where(p => p.StudentApplicationId == bill.StudentApplicationId && p.PaymentStatus == "Success")
                .SumAsync(p => p.AmountPaid);
            var remainingBalance = bill.Application.Program.BaseFee - totalPaidSoFar;
            if (remainingBalance < 0) remainingBalance = 0;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial).FontColor(Colors.Grey.Darken3));

                    page.Background().AlignCenter().AlignMiddle().Text("PAID").FontSize(120).FontColor(Colors.Grey.Lighten4).Bold();

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("Bluelines Tech Solutions").FontSize(22).Bold().FontColor("#0d6efd");
                            col.Item().Text("Rajiv Nagar 6th Street, Kovilpatti, TN").FontColor(Colors.Grey.Medium);
                            col.Item().Text("info@bluelinestechsolutions.com | +91 7373 005 005").FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(200).AlignRight().Column(col =>
                        {
                            col.Item().AlignRight().Text("RECEIPT").FontSize(24).Bold().FontColor(Colors.Grey.Lighten2).LetterSpacing(0.1f);
                            col.Item().AlignRight().Text($"Receipt No: {bill.PaymentGatewayReference}").Bold();
                            col.Item().AlignRight().Text($"Date: {bill.PaymentDate:dd MMM yyyy, HH:mm}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Inch).Column(col =>
                    {
                        col.Item().Background(Colors.Grey.Lighten4).Padding(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("BILLED TO").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                                c.Item().Text($"ID: {bill.Application.Student.StudentId ?? "N/A"}").FontSize(10).Bold().FontColor("#0d6efd");
                                c.Item().Text(bill.Application.Student.FullName).FontSize(14).Bold().FontColor(Colors.Black);
                                c.Item().Text(secureEmail);
                                c.Item().Text(bill.Application.Student.CollegeName);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text("PAYMENT INFORMATION").FontSize(9).Bold().FontColor(Colors.Grey.Medium);
                                c.Item().AlignRight().Text($"Method: {bill.PaymentMethod}");
                                c.Item().AlignRight().Text($"Status: {bill.PaymentStatus}").FontColor(Colors.Green.Medium).Bold();
                            });
                        });

                        col.Item().PaddingTop(30).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(120);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(2).BorderColor(Colors.Black).PaddingBottom(5).Text("DESCRIPTION").FontSize(10).Bold();
                                header.Cell().BorderBottom(2).BorderColor(Colors.Black).PaddingBottom(5).AlignRight().Text("AMOUNT").FontSize(10).Bold();
                            });

                            table.Cell().PaddingTop(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(15).Column(c => {
                                c.Item().Text("Program Enrollment").Bold().FontSize(12).FontColor(Colors.Black);
                                c.Item().Text($"{bill.Application.Program.Title} ({bill.Application.Program.Type})").FontColor(Colors.Grey.Medium);
                            });

                            table.Cell().PaddingTop(15).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(15).AlignRight()
                                .Text($"₹{bill.AmountPaid:N2}").Bold().FontSize(12).FontColor(Colors.Black);
                        });

                        col.Item().PaddingTop(20).AlignRight().Column(c => {
                            c.Item().AlignRight().Text($"Program Total Fee: ₹{bill.Application.Program.BaseFee:N2}").FontSize(11);
                            c.Item().AlignRight().Text($"Total Paid So Far: ₹{totalPaidSoFar:N2}").FontSize(11);
                            c.Item().PaddingBottom(10).AlignRight().Text($"Remaining Balance: ₹{remainingBalance:N2}").FontSize(12).Bold().FontColor(Colors.Red.Medium);

                            c.Item().AlignRight().Text($"Amount Paid This Transaction: ₹{bill.AmountPaid:N2}").FontSize(16).Bold().FontColor("#0d6efd");
                        });
                    });

                    page.Footer().AlignCenter().Column(col => {
                        col.Item().AlignCenter().Text("Thank you for registering with Bluelines Tech Solutions!").Bold();
                        col.Item().AlignCenter().Text("This is a computer-generated receipt and requires no physical signature.").FontColor(Colors.Grey.Medium);
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Receipt_{bill.PaymentGatewayReference}.pdf");
        }
    }
}