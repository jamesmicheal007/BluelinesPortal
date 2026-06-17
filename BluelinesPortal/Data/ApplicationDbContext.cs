using BluelinesPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BluelinesPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add your new tables here
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<ProgramItem> Programs { get; set; }
        public DbSet<StudentApplication> Applications { get; set; }
        public DbSet<PaymentRecord> Payments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
    }
}