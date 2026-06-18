using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class ProjectMaterial
    {
        [Key]
        public int Id { get; set; }

        public int ProgramItemId { get; set; }
        [ForeignKey("ProgramItemId")]
        public ProgramItem Program { get; set; }

        public ProjectAssetType AssetType { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } // e.g. "Phase 1 Abstract", "Final Source Code"

        [StringLength(500)]
        public string? FilePath { get; set; } // Path to uploaded PDF, ZIP, or PPT

        [StringLength(500)]
        public string? ExternalUrl { get; set; } // YouTube Link or Google Drive Link

        // If true, only Enrolled students can see this. If false, public users can see it as a teaser.
        public bool IsPremium { get; set; } = true;
    }
}