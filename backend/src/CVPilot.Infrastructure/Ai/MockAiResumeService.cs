using CVPilot.Application.Common.Interfaces;
using CVPilot.Domain.Entities;

namespace CVPilot.Infrastructure.Ai;

public class MockAiResumeService : IAiResumeService
{
    public string ProviderName => "Development/Mock";

    public Task<JobAnalysisResultDto> AnalyzeJobDescriptionAsync(string rawJobText, CancellationToken ct = default)
    {
        // Simple heuristic extraction for mock/development environment
        var lower = rawJobText.ToLowerInvariant();
        var extractedSkills = new List<string>();

        var skillCatalog = new[] { "C#", ".NET", "ASP.NET Core", "SQL Server", "Angular", "TypeScript", "JavaScript", "Docker", "REST API", "Git", "Azure", "HTML", "CSS", "Bootstrap", "Tailwind" };
        foreach (var s in skillCatalog)
        {
            if (lower.Contains(s.ToLowerInvariant()))
            {
                extractedSkills.Add(s);
            }
        }

        if (extractedSkills.Count == 0)
        {
            extractedSkills.AddRange(new[] { "Software Development", "Problem Solving", "Team Collaboration" });
        }

        var required = extractedSkills.Take(Math.Max(1, (int)(extractedSkills.Count * 0.7))).ToList();
        var preferred = extractedSkills.Skip(required.Count).ToList();

        var jobTitle = "Software Developer";
        if (lower.Contains("full stack") || lower.Contains("fullstack")) jobTitle = "Full Stack Developer";
        else if (lower.Contains(".net") || lower.Contains("c#") || lower.Contains("asp.net")) jobTitle = "ASP.NET Core Developer";
        else if (lower.Contains("frontend") || lower.Contains("angular")) jobTitle = "Frontend Developer";

        var result = new JobAnalysisResultDto(
            JobTitle: jobTitle,
            CompanyName: null,
            RequiredSkills: required,
            PreferredSkills: preferred,
            ExperienceRequirement: lower.Contains("year") ? "1-3 years" : "Entry to Mid level",
            EducationRequirements: new List<string> { "Bachelor's degree in Computer Science, Software Engineering or related field" },
            Responsibilities: new List<string>
            {
                "Design, develop, and maintain high-quality web applications",
                "Collaborate with cross-functional teams to define and ship new features",
                "Ensure application performance, quality, and responsiveness",
                "Participate in code reviews and architectural discussions"
            },
            Keywords: extractedSkills
        );

        return Task.FromResult(result);
    }

    public Task<ProfileMatchResultDto> MatchProfileToJobAsync(UserProfile profile, JobDescription job, CancellationToken ct = default)
    {
        var userSkillNames = profile.Skills.Select(s => s.Name.Trim().ToLowerInvariant()).ToHashSet();

        // Parse required skills from job
        var requiredSkills = ParseJsonList(job.RequiredSkillsJson);
        var preferredSkills = ParseJsonList(job.PreferredSkillsJson);

        var allJobSkills = requiredSkills.Concat(preferredSkills).Distinct().ToList();
        var confirmedMatches = new List<string>();
        var relatedSkills = new List<string>();
        var missingSkills = new List<string>();

        foreach (var jobSkill in allJobSkills)
        {
            var lowerJobSkill = jobSkill.ToLowerInvariant();
            if (userSkillNames.Contains(lowerJobSkill))
            {
                confirmedMatches.Add(jobSkill);
            }
            else if (userSkillNames.Any(u => u.Contains(lowerJobSkill) || lowerJobSkill.Contains(u)))
            {
                relatedSkills.Add(jobSkill);
            }
            else
            {
                missingSkills.Add(jobSkill);
            }
        }

        var total = allJobSkills.Count == 0 ? 1 : allJobSkills.Count;
        var matchScore = Math.Round((decimal)(confirmedMatches.Count * 100 + relatedSkills.Count * 50) / total, 1);
        if (matchScore > 100) matchScore = 100;

        var recommendations = missingSkills.Select(s => $"Consider gaining familiarity or adding verified projects demonstrating {s}").ToList();

        var result = new ProfileMatchResultDto(
            MatchScore: matchScore,
            ConfirmedMatches: confirmedMatches,
            RelatedExperienceSkills: relatedSkills,
            MissingSkills: missingSkills,
            Recommendations: recommendations,
            SummaryRationale: $"Profile matches {confirmedMatches.Count} of {allJobSkills.Count} target requirements with {relatedSkills.Count} related capabilities."
        );

        return Task.FromResult(result);
    }

    public Task<TailoredResumeContentDto> GenerateTailoredResumeAsync(UserProfile profile, JobDescription job, CancellationToken ct = default)
    {
        // Fact-constrained generation: only use profile data
        var confirmedSkills = profile.Skills.Select(s => s.Name).ToList();

        var highlightedExperiences = profile.Experiences
            .OrderBy(e => e.DisplayOrder)
            .Select(e =>
            {
                var bullets = ParseJsonList(e.BulletPointsJson);
                var tailoredBullets = bullets.Select(b => $"• {b}").ToList();
                if (tailoredBullets.Count == 0)
                {
                    tailoredBullets.Add($"• Contributed to {e.CompanyName} as {e.JobTitle}, delivering assigned responsibilities effectively.");
                }

                return new TailoredExperienceDto(
                    ExperienceId: e.Id,
                    JobTitle: e.JobTitle,
                    CompanyName: e.CompanyName,
                    TailoredBulletPoints: tailoredBullets
                );
            }).ToList();

        var highlightedProjects = profile.Projects
            .OrderBy(p => p.DisplayOrder)
            .Select(p =>
            {
                var bullets = ParseJsonList(p.BulletPointsJson);
                var tailoredBullets = bullets.Select(b => $"• {b}").ToList();
                if (tailoredBullets.Count == 0 && !string.IsNullOrWhiteSpace(p.Description))
                {
                    tailoredBullets.Add($"• {p.Description}");
                }

                return new TailoredProjectDto(
                    ProjectId: p.Id,
                    Title: p.Title,
                    Description: p.Description,
                    TailoredBulletPoints: tailoredBullets
                );
            }).ToList();

        var summary = !string.IsNullOrWhiteSpace(profile.Summary)
            ? profile.Summary
            : $"Dedicated professional with confirmed experience in {string.Join(", ", confirmedSkills.Take(4))}, focused on delivering reliable software solutions for {job.JobTitle}.";

        var result = new TailoredResumeContentDto(
            TargetRole: job.JobTitle,
            ProfessionalSummary: summary,
            HighlightedExperiences: highlightedExperiences,
            PrioritizedSkills: confirmedSkills,
            HighlightedProjects: highlightedProjects
        );

        return Task.FromResult(result);
    }

    public Task<CoverLetterResultDto> GenerateCoverLetterAsync(UserProfile profile, JobDescription job, CancellationToken ct = default)
    {
        var company = string.IsNullOrWhiteSpace(job.CompanyName) ? "your organization" : job.CompanyName;
        var confirmedSkills = profile.Skills.Select(s => s.Name).Take(4).ToList();
        var skillsMention = confirmedSkills.Count > 0 ? string.Join(", ", confirmedSkills) : "software development";

        var body = $"I am writing to express my strong interest in the {job.JobTitle} position at {company}. " +
                   $"With my background and confirmed expertise in {skillsMention}, I am confident in my ability to make an immediate, positive impact on your team.\n\n" +
                   $"Throughout my career, I have prioritized high-quality engineering, collaborative teamwork, and continuous improvement. " +
                   $"I welcome the opportunity to discuss how my qualifications align with your engineering goals.\n\n" +
                   $"Thank you for your time and consideration.";

        var result = new CoverLetterResultDto(
            Subject: $"Application for {job.JobTitle} - {profile.User?.FullName ?? "Applicant"}",
            Salutation: $"Dear Hiring Team at {company},",
            Body: body,
            Closing: "Sincerely,\n" + (profile.User?.FullName ?? "Applicant")
        );

        return Task.FromResult(result);
    }

    public Task<SkillGapAnalysisDto> GenerateSkillGapAnalysisAsync(UserProfile profile, JobDescription job, CancellationToken ct = default)
    {
        var userSkills = profile.Skills.Select(s => s.Name.ToLowerInvariant()).ToHashSet();
        var requiredSkills = ParseJsonList(job.RequiredSkillsJson);
        var missing = requiredSkills.Where(r => !userSkills.Contains(r.ToLowerInvariant())).ToList();

        var learningPath = missing.Select(m => $"Complete an accredited hands-on tutorial or build a sample project featuring {m}").ToList();
        var tips = new List<string>
        {
            "Build a demonstrable portfolio repository highlighting your strongest projects.",
            "Include measurable impact (e.g. latency reduced, user adoption, test coverage) in your experience descriptions.",
            "Highlight core computer science fundamentals and architecture patterns."
        };

        return Task.FromResult(new SkillGapAnalysisDto(
            MissingCriticalSkills: missing,
            RecommendedLearningPath: learningPath,
            ActionableTips: tips
        ));
    }

    private static List<string> ParseJsonList(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return new List<string>();
        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
            return parsed ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

