using CVPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVPilot.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<WorkExperience> WorkExperiences { get; }
    DbSet<Education> Educations { get; }
    DbSet<UserSkill> UserSkills { get; }
    DbSet<Project> Projects { get; }
    DbSet<Certification> Certifications { get; }
    DbSet<Language> Languages { get; }
    DbSet<JobDescription> JobDescriptions { get; }
    DbSet<Resume> Resumes { get; }
    DbSet<ResumeVersion> ResumeVersions { get; }
    DbSet<ResumeJobMatch> ResumeJobMatches { get; }
    DbSet<CoverLetter> CoverLetters { get; }
    DbSet<DocumentExport> DocumentExports { get; }
    DbSet<UsageRecord> UsageRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

