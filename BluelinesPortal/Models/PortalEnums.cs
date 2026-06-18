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

    public enum DiscountType
    {
        None,
        Percentage,
        FixedAmount
    }

    public enum ProjectAssetType
    {
        Abstract,
        PowerPoint,
        ReviewDocument,
        FinalDocument,
        SourceCode,
        Screenshots,
        HowToRunVideo,
        IntroVideo,
        Other
    }

    public enum ApplicationStatus
    {
        Pending,
        UnderReview,
        Approved,
        Enrolled,
        Rejected
    }
}