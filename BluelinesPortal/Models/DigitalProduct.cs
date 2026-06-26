using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class DigitalProduct
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public ProductCategory Category { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public bool IsFree { get; set; } = false;

        // --- PROJECT SPECIFICS ---
        public string? ShortDescription { get; set; }
        public string? Abstract { get; set; }

        [StringLength(200)] public string? FrontendTech { get; set; }
        [StringLength(200)] public string? BackendTech { get; set; }
        [StringLength(200)] public string? DatabaseTech { get; set; }

        [StringLength(500)] public string? YouTubeDemoUrl { get; set; }
        [StringLength(500)] public string? ThumbnailPath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public ICollection<ProductAsset> Assets { get; set; } = new List<ProductAsset>();
        // --- PRICING UPGRADES ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; } // e.g., ₹5000 (for strikethrough)

        // --- ADVANCED DETAILS ---
        [StringLength(200)] public string? ApplicableFor { get; set; } // e.g., B.Tech, M.Tech, BCA, MCA
        [StringLength(500)] public string? Modules { get; set; } // e.g., Admin, User, Seller
        [StringLength(50)] public string DeliveryTime { get; set; } = "Instant";
    }
}