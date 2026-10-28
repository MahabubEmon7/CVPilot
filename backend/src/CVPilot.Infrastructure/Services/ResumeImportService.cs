using System.Text;
using System.Text.RegularExpressions;
using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Profile.DTOs;
using CVPilot.Application.Features.Profile.Interfaces;
using CVPilot.Domain.Entities;
using CVPilot.Domain.Enums;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace CVPilot.Infrastructure.Services;

public class ResumeImportService : IResumeImportService
{
    private readonly IAppDbContext _dbContext;
    private readonly IProfileService _profileService;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt"
    };

    public ResumeImportService(IAppDbContext dbContext, IProfileService profileService)
    {
        _dbContext = dbContext;
        _profileService = profileService;
    }

    public async Task<Result<ParsedResumeDto>> ExtractResumeDataAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            return Result<ParsedResumeDto>.Failure("Unsupported file format. Please upload a PDF, DOCX, or TXT file.");
        }

        string extractedText = string.Empty;

        try
        {
            if (ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                extractedText = ExtractTextFromPdf(fileStream);
            }
            else if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                extractedText = ExtractTextFromDocx(fileStream);
            }
            else
            {
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                extractedText = await reader.ReadToEndAsync(ct);
            }
        }
        catch (Exception ex)
        {
            return Result<ParsedResumeDto>.Failure($"Failed to read file: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            return Result<ParsedResumeDto>.Failure("Could not extract any readable text from the document. Please ensure it is not an image-only scan.");
        }

        var parsed = ParseRawText(extractedText);
        return Result<ParsedResumeDto>.Success(parsed);
    }

    public async Task<Result<UserProfileDto>> ConfirmAndImportToProfileAsync(Guid userId, ConfirmImportProfileDto dto, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
        {
            return Result<UserProfileDto>.Failure("Profile not found.");
        }

        // 1. Update personal details if provided
        if (!string.IsNullOrWhiteSpace(dto.FullName) && profile.User != null)
        {
            profile.User.FullName = dto.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Headline)) profile.Headline = dto.Headline.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Summary)) profile.Summary = dto.Summary.Trim();
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) profile.PhoneNumber = dto.PhoneNumber.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LocationCity)) profile.LocationCity = dto.LocationCity.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LocationCountry)) profile.LocationCountry = dto.LocationCountry.Trim();
        if (!string.IsNullOrWhiteSpace(dto.LinkedInUrl)) profile.LinkedInUrl = dto.LinkedInUrl.Trim();
        if (!string.IsNullOrWhiteSpace(dto.GitHubUrl)) profile.GitHubUrl = dto.GitHubUrl.Trim();

        // 2. Add confirmed experiences
        if (dto.ConfirmedExperiences != null)
        {
            foreach (var exp in dto.ConfirmedExperiences)
            {
                if (string.IsNullOrWhiteSpace(exp.JobTitle) || string.IsNullOrWhiteSpace(exp.CompanyName)) continue;

                var bullets = exp.BulletPoints ?? new List<string>();
                profile.Experiences.Add(new WorkExperience
                {
                    UserProfileId = profile.Id,
                    JobTitle = exp.JobTitle.Trim(),
                    CompanyName = exp.CompanyName.Trim(),
                    Location = exp.Location?.Trim(),
                    StartDate = exp.StartDate == default ? DateTime.UtcNow.AddYears(-1) : exp.StartDate,
                    EndDate = exp.IsCurrent ? null : exp.EndDate,
                    IsCurrent = exp.IsCurrent,
                    BulletPointsJson = System.Text.Json.JsonSerializer.Serialize(bullets),
                    DisplayOrder = profile.Experiences.Count
                });
            }
        }

        // 3. Add confirmed education
        if (dto.ConfirmedEducations != null)
        {
            foreach (var edu in dto.ConfirmedEducations)
            {
                if (string.IsNullOrWhiteSpace(edu.Institution) || string.IsNullOrWhiteSpace(edu.Degree)) continue;

                profile.Educations.Add(new Education
                {
                    UserProfileId = profile.Id,
                    Institution = edu.Institution.Trim(),
                    Degree = edu.Degree.Trim(),
                    FieldOfStudy = edu.FieldOfStudy?.Trim() ?? "General",
                    StartDate = edu.StartDate == default ? DateTime.UtcNow.AddYears(-4) : edu.StartDate,
                    EndDate = edu.IsCurrent ? null : edu.EndDate,
                    IsCurrent = edu.IsCurrent,
                    GradeOrGpa = edu.GradeOrGpa?.Trim(),
                    Description = edu.Description?.Trim(),
                    DisplayOrder = profile.Educations.Count
                });
            }
        }

        // 4. Add confirmed skills (avoid duplicates)
        if (dto.ConfirmedSkills != null)
        {
            var existingNames = profile.Skills.Select(s => s.Name.ToLowerInvariant()).ToHashSet();
            foreach (var sk in dto.ConfirmedSkills)
            {
                var name = sk.Name.Trim();
                if (string.IsNullOrWhiteSpace(name) || existingNames.Contains(name.ToLowerInvariant())) continue;

                profile.Skills.Add(new UserSkill
                {
                    UserProfileId = profile.Id,
                    Name = name,
                    Category = sk.Category,
                    Proficiency = sk.Proficiency,
                    YearsOfExperience = sk.YearsOfExperience
                });
                existingNames.Add(name.ToLowerInvariant());
            }
        }

        await _dbContext.SaveChangesAsync(ct);

        return await _profileService.GetProfileByUserIdAsync(userId, ct);
    }

    private static string ExtractTextFromPdf(Stream stream)
    {
        var sb = new StringBuilder();
        using var pdf = PdfDocument.Open(stream);
        foreach (var page in pdf.GetPages())
        {
            sb.AppendLine(page.Text);
        }
        return sb.ToString();
    }

    private static string ExtractTextFromDocx(Stream stream)
    {
        var sb = new StringBuilder();
        using var wordDoc = WordprocessingDocument.Open(stream, false);
        var body = wordDoc.MainDocumentPart?.Document?.Body;
        if (body != null)
        {
            foreach (var para in body.Descendants<Paragraph>())
            {
                if (!string.IsNullOrWhiteSpace(para.InnerText))
                {
                    sb.AppendLine(para.InnerText);
                }
            }
        }
        return sb.ToString();
    }

    private static ParsedResumeDto ParseRawText(string text)
    {
        var emailMatch = Regex.Match(text, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
        var phoneMatch = Regex.Match(text, @"(\+?\d{1,3}[-.\s]?)?\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4,}");

        // Heuristic skill extraction
        var commonSkills = new[]
        {
            "C#", ".NET", "ASP.NET Core", "SQL Server", "Angular", "TypeScript", "JavaScript",
            "React", "Vue", "Node.js", "Docker", "Kubernetes", "Git", "Azure", "AWS", "Python",
            "Java", "HTML", "CSS", "SCSS", "REST API", "Microservices", "CI/CD", "Problem Solving",
            "Agile", "Scrum", "OOP", "Entity Framework", "Bootstrap", "Tailwind"
        };

        var extractedSkills = new List<CreateUserSkillDto>();
        var lowerText = text.ToLowerInvariant();
        foreach (var skill in commonSkills)
        {
            if (lowerText.Contains(skill.ToLowerInvariant()))
            {
                var cat = (skill is "Problem Solving" or "Agile" or "Scrum") 
                    ? SkillCategory.Soft 
                    : (skill is "Git" or "Docker" or "Kubernetes" or "Azure" or "AWS") 
                        ? SkillCategory.ToolOrFramework 
                        : SkillCategory.Technical;

                extractedSkills.Add(new CreateUserSkillDto(skill, cat, SkillProficiency.Intermediate, null));
            }
        }

        // Extract first 2-3 lines as candidate name / headline
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 2)
            .Take(10)
            .ToList();

        string candidateName = lines.Count > 0 ? lines[0] : string.Empty;
        if (candidateName.Length > 50 || candidateName.Contains('@')) candidateName = string.Empty;

        string candidateHeadline = lines.Count > 1 ? lines[1] : string.Empty;
        if (candidateHeadline.Length > 80 || candidateHeadline.Contains('@')) candidateHeadline = "Software Professional";

        return new ParsedResumeDto(
            FullName: string.IsNullOrWhiteSpace(candidateName) ? null : candidateName,
            Email: emailMatch.Success ? emailMatch.Value : null,
            PhoneNumber: phoneMatch.Success ? phoneMatch.Value : null,
            LocationCity: lowerText.Contains("dhaka") ? "Dhaka" : null,
            LocationCountry: "Bangladesh",
            Headline: candidateHeadline,
            Summary: null,
            Experiences: new List<CreateWorkExperienceDto>(),
            Educations: new List<CreateEducationDto>(),
            Skills: extractedSkills,
            RawExtractedText: text.Length > 3000 ? text.Substring(0, 3000) + "..." : text
        );
    }
}
