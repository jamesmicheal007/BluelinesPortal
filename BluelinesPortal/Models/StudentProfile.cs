using System.ComponentModel.DataAnnotations;

namespace BluelinesPortal.Models
{
    public class StudentProfile
    {
        [Key]
        public int Id { get; set; }

        public string IdentityUserId { get; set; }

        [StringLength(20)]
        public string? StudentId { get; set; }

        [Required]
        public string FullName { get; set; }

        public string? PhoneNumber { get; set; }

        // --- MISSING PROPERTIES CAUSING UI ERRORS ---
        public string? Degree { get; set; }
        public string? YearOfStudy { get; set; }
        public string? CurrentTechStack { get; set; }
        public string? GitHubProfileUrl { get; set; }

        public ICollection<StudentApplication> Applications { get; set; } = new List<StudentApplication>();

        // --- ADMISSION FORM PROPERTIES ---
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Pincode { get; set; }

        public string? CollegeName { get; set; }
        public string? DegreeProgram { get; set; }
        public string? CurrentYear { get; set; }
    }
}