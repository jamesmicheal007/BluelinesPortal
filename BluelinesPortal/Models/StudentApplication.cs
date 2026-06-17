using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BluelinesPortal.Models
{
    public class StudentApplication
    {
        [Key]
        public int Id { get; set; }

        public int StudentProfileId { get; set; }
        [ForeignKey("StudentProfileId")]
        public StudentProfile Student { get; set; }

        // --- FIXED: Renamed to match the UI's expectation of 'ProgramItemId' ---
        public int ProgramItemId { get; set; }
        [ForeignKey("ProgramItemId")]
        public ProgramItem Program { get; set; }

        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        // --- FIXED: Missing AdminNotes Property ---
        public string? AdminNotes { get; set; }

        // --- QUESTIONNAIRE FIELDS ---
        [Range(1, 5)] public int SkillHtmlCss { get; set; }
        [Range(1, 5)] public int SkillJavaScript { get; set; }
        [Range(1, 5)] public int SkillPython { get; set; }
        [Range(1, 5)] public int SkillSql { get; set; }

        public string? ProjectDescription { get; set; }
        public string? ProblemSolvingApproach { get; set; }
        public string? TechInterests { get; set; }
        public string? MainGoal { get; set; }

        public bool HasLaptop { get; set; }

        public string? InternshipExpectations { get; set; }
    }
}