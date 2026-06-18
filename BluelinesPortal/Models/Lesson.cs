using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        public int ModuleId { get; set; }
        [ForeignKey("ModuleId")]
        public Module Module { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; }

        public string? Content { get; set; } // Text / HTML Content

        [StringLength(500)]
        public string? VideoUrl { get; set; } // YouTube / Embedded Video

        // --- NEW FIELDS FOR ADVANCED ONLINE COURSES ---
        [StringLength(500)]
        public string? PdfDocumentPath { get; set; } // Course Notes PDF

        [StringLength(500)]
        public string? DownloadableAssetPath { get; set; } // ZIP files, starter code, etc.

        public int OrderIndex { get; set; }
        public bool IsFreePreview { get; set; } = false;
    }
}