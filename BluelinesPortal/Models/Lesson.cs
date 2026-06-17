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

        [Required]
        [StringLength(150)]
        public string Title { get; set; } // e.g., "Setting up Visual Studio"

        public string Content { get; set; } // Detailed tutorial text/HTML

        [StringLength(255)]
        public string VideoUrl { get; set; } // YouTube/Vimeo embed URL

        public int OrderIndex { get; set; }

        // THE FREEMIUM TOGGLE
        public bool IsFreePreview { get; set; } = false;
    }
}