using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class ProductOrder
    {
        [Key]
        public int Id { get; set; }

        // Link to the Student
        public int StudentProfileId { get; set; }
        [ForeignKey("StudentProfileId")]
        public StudentProfile Student { get; set; }

        // Link to the Product
        public int DigitalProductId { get; set; }
        [ForeignKey("DigitalProductId")]
        public DigitalProduct Product { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)] public string PaymentMethod { get; set; }
        [StringLength(100)] public string? UTRNumber { get; set; }
        [StringLength(255)] public string? ScreenshotPath { get; set; }

        // Status: "PendingVerification", "Success", "Rejected"
        [StringLength(50)] public string OrderStatus { get; set; } = "PendingVerification";
        // --- ADVANCED CHECKOUT TRACKING ---
        public string? SelectedAddOns { get; set; } // Store as comma-separated string (e.g., "Explanation,Installation")

        [Column(TypeName = "decimal(18,2)")]
        public decimal AddOnTotal { get; set; }

        public bool IsSplitPayment { get; set; } = false; // True if they chose to pay 50% now

        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceDue { get; set; } // The remaining 50% to be paid on delivery
        // --- SECOND PAYMENT TRACKING ---
        [StringLength(100)] public string? BalanceUTRNumber { get; set; }
        [StringLength(255)] public string? BalanceScreenshotPath { get; set; }
        [StringLength(50)] public string BalanceStatus { get; set; } = "None"; // "None", "PendingVerification", "Paid"
    }
}