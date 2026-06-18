using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class ProgramItem
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; }

        public ProgramType Type { get; set; }
        public int DurationInDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseFee { get; set; }
        public bool IsActive { get; set; } = true;

        // --- NEW: DISCOUNT & COUPON SYSTEM ---
        public bool IsDiscountActive { get; set; } = false;
        public DiscountType DiscountType { get; set; } = DiscountType.None;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; } = 0; // The % or ₹ amount

        [StringLength(50)]
        public string? CouponCode { get; set; } // If NULL, discount applies automatically. If set, student must type this code.

        // Content
        [StringLength(200)] public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        [StringLength(255)] public string? ThumbnailPath { get; set; }
        [StringLength(255)] public string? BrochurePath { get; set; }
        [StringLength(255)] public string? YouTubeVideoUrl { get; set; }
        [StringLength(500)] public string? Prerequisites { get; set; }

        // Navigation for Project Assets
        public ICollection<ProjectMaterial> ProjectMaterials { get; set; } = new List<ProjectMaterial>();
    }
}