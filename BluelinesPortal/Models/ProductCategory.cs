using System.ComponentModel.DataAnnotations;

namespace BluelinesPortal.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // e.g., "IEEE Projects", "HTML Templates"

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<DigitalProduct> Products { get; set; } = new List<DigitalProduct>();
    }
}