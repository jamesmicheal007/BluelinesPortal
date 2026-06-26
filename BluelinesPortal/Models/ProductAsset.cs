using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class ProductAsset
    {
        [Key]
        public int Id { get; set; }

        public int DigitalProductId { get; set; }
        [ForeignKey("DigitalProductId")]
        public DigitalProduct Product { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } // e.g., "Full Source Code", "Synopsis"

        public string? FilePath { get; set; } // /uploads/products/files/...
        public string? ExternalLink { get; set; } // Google Drive link

        // If true, only buyers who have a 'Success' order can download it.
        public bool IsPremium { get; set; } = true;
    }
}