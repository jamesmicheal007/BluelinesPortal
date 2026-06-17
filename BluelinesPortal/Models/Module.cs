using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class Module
    {
        [Key]
        public int Id { get; set; }

        public int ProgramItemId { get; set; }
        [ForeignKey("ProgramItemId")]
        public ProgramItem Program { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } // e.g., "Week 1: Introduction to C#"

        public int OrderIndex { get; set; } // To sort modules correctly

        public ICollection<Lesson> Lessons { get; set; }
    }
}