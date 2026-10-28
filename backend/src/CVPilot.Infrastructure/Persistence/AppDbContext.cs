using CVPilot.Application.Common.Interfaces;
using CVPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVPilot.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Education> Educations => Set<Education>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<JobDescription> JobDescriptions => Set<JobDescription>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<ResumeVersion> ResumeVersions => Set<ResumeVersion>();
    public DbSet<ResumeJobMatch> ResumeJobMatches => Set<ResumeJobMatch>();
    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();
    public DbSet<DocumentExport> DocumentExports => Set<DocumentExport>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();

            entity.HasOne(e => e.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Headline).HasMaxLength(250);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.LocationCity).HasMaxLength(100);
            entity.Property(e => e.LocationCountry).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(500);
            entity.Property(e => e.GitHubUrl).HasMaxLength(500);
            entity.Property(e => e.PortfolioUrl).HasMaxLength(500);

            entity.HasMany(p => p.Experiences)
                .WithOne(e => e.UserProfile)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Educations)
                .WithOne(e => e.UserProfile)
                .HasForeignKey(e => e.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Skills)
                .WithOne(s => s.UserProfile)
                .HasForeignKey(s => s.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Projects)
                .WithOne(pr => pr.UserProfile)
                .HasForeignKey(pr => pr.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Certifications)
                .WithOne(c => c.UserProfile)
                .HasForeignKey(c => c.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Languages)
                .WithOne(l => l.UserProfile)
                .HasForeignKey(l => l.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkExperience configuration
        modelBuilder.Entity<WorkExperience>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobTitle).HasMaxLength(150).IsRequired();
            entity.Property(e => e.CompanyName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Location).HasMaxLength(100);
        });

        // Education configuration
        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Institution).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Degree).HasMaxLength(150).IsRequired();
            entity.Property(e => e.FieldOfStudy).HasMaxLength(150).IsRequired();
            entity.Property(e => e.GradeOrGpa).HasMaxLength(50);
        });

        // UserSkill configuration
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => new { e.UserProfileId, e.Name }).IsUnique();
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TechnologiesUsed).HasMaxLength(300);
            entity.Property(e => e.RepoUrl).HasMaxLength(500);
            entity.Property(e => e.LiveUrl).HasMaxLength(500);
        });

        // JobDescription configuration
        modelBuilder.Entity<JobDescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.JobTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.JobDescriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Resume configuration
        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TargetRole).HasMaxLength(150);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Resumes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.JobDescription)
                .WithMany(j => j.Resumes)
                .HasForeignKey(e => e.JobDescriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.Resume)
                .HasForeignKey(v => v.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.JobMatches)
                .WithOne(m => m.Resume)
                .HasForeignKey(m => m.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Exports)
                .WithOne(ex => ex.Resume)
                .HasForeignKey(ex => ex.ResumeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ResumeJobMatch configuration
        modelBuilder.Entity<ResumeJobMatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MatchPercentage).HasPrecision(5, 2);
        });

        // CoverLetter configuration
        modelBuilder.Entity<CoverLetter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.TargetRole).HasMaxLength(150);

            entity.HasOne(e => e.User)
                .WithMany(u => u.CoverLetters)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.JobDescription)
                .WithMany(j => j.CoverLetters)
                .HasForeignKey(e => e.JobDescriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // UsageRecord configuration
        modelBuilder.Entity<UsageRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FeatureName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Provider).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UsageRecords)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<CVPilot.Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
