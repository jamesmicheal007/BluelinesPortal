using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class Certificate
    {
        [Key]
        [StringLength(25)]
        public string VerificationId { get; set; } // e.g., "BLT-2026-A1B2C3"

        public int StudentApplicationId { get; set; }
        [ForeignKey("StudentApplicationId")]
        public StudentApplication Application { get; set; }

        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;
    }
}