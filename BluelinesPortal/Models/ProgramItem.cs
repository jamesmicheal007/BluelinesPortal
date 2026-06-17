using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class ProgramItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        public ProgramType Type { get; set; }
        public int DurationInDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseFee { get; set; } = decimal.Zero;
        public bool IsActive { get; set; } = true;

        // --- NEW FIELDS ---
        [StringLength(200)]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; } // Detailed Description

        [StringLength(255)]
        public string? ThumbnailPath { get; set; } // Path to uploaded image

        [StringLength(255)]
        public string? BrochurePath { get; set; } // Path to uploaded PDF

        [StringLength(255)]
        public string? YouTubeVideoUrl { get; set; }

        [StringLength(500)]
        public string? Prerequisites { get; set; } // e.g., "Basic C# knowledge"
    }
}