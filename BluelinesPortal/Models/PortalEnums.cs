namespace BluelinesPortal.Models
{
    public enum ProgramType
    {
        Internship,
        MiniProject,
        MajorProject,
        OnlineCourse,
        OfflineCourse,
        TechnicalTuition
    }

    public enum ApplicationStatus
    {
        Pending,       // Student just applied
        UnderReview,   // Admin is checking
        Approved,      // Offer extended
        Rejected,      // Not accepted
        Enrolled       // Fee paid, active student
    }
}