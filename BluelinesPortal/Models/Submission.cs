using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class Submission
    {
        [Key]
        public int Id { get; set; }

        public int StudentApplicationId { get; set; }
        [ForeignKey("StudentApplicationId")]
        public StudentApplication Application { get; set; }

        // Added from ProjectSubmission: Great for multi-phase projects
        [Required, StringLength(150)]
        public string SubmissionTitle { get; set; }

        [Required]
        [StringLength(255)]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string GitHubLink { get; set; }

        // Added from ProjectSubmission: Great for UI/UX or document uploads
        [Url(ErrorMessage = "Please enter a valid URL")]
        [StringLength(255)]
        public string CloudDriveLink { get; set; }

        public string StudentNotes { get; set; }

        public string MentorFeedback { get; set; }

        [StringLength(50)]
        public string ReviewStatus { get; set; } = "Pending"; // "Pending", "Needs Revision", "Approved"

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

        // Essential for tracking when the Admin graded it
        public DateTime? ReviewedOn { get; set; }
    }
}