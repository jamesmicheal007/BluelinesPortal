using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class PaymentRecord
    {
        [Key]
        public int Id { get; set; }

        public int StudentApplicationId { get; set; }
        [ForeignKey("StudentApplicationId")]
        public StudentApplication Application { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string PaymentMethod { get; set; } // Cash, GPay, Bank Transfer

        [StringLength(100)]
        public string PaymentGatewayReference { get; set; } // UTR / Manual Ref

        public string PaymentStatus { get; set; } // "Success", "PendingVerification", "Rejected"

        // === 💡 THE FIX: Add the '?' to make this optional in SQL ===
        [StringLength(255)]
        public string? ScreenshotPath { get; set; }
    }
}