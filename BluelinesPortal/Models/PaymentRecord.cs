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

        [StringLength(100)]
        public string PaymentGatewayReference { get; set; } // e.g., Razorpay 'pay_XXXXXXX' ID

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Success"; // Success, Failed, Refunded
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Online"; // Online, Cash, GPay, Account Transfer

        [StringLength(255)]
        public string ScreenshotPath { get; set; } // Path to the uploaded image file
    }
}