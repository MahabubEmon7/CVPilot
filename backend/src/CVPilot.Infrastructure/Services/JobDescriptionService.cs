using System.Text.Json;
using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.JobDescriptions.DTOs;
using CVPilot.Application.Features.JobDescriptions.Interfaces;
using CVPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVPilot.Infrastructure.Services;

public class JobDescriptionService : IJobDescriptionService
{
    private readonly IAppDbContext _dbContext;
    private readonly IAiResumeService _aiService;

    public JobDescriptionService(IAppDbContext dbContext, IAiResumeService aiService)
    {
        _dbContext = dbContext;
        _aiService = aiService;
    }

    public async Task<Result<JobDescriptionDto>> CreateAndAnalyzeAsync(Guid userId, CreateJobDescriptionRequestDto request, CancellationToken ct = default)
    {
        var rawContent = request.RawContent.Trim();

        // Trigger AI analysis (Mock or configured provider)
        var analysis = await _aiService.AnalyzeJobDescriptionAsync(rawContent, ct);

        var finalJobTitle = !string.IsNullOrWhiteSpace(request.JobTitle)
            ? request.JobTitle.Trim()
            : !string.IsNullOrWhiteSpace(analysis.JobTitle)
                ? analysis.JobTitle
                : "Untitled Job Position";

        var finalCompany = !string.IsNullOrWhiteSpace(request.CompanyName)
            ? request.CompanyName.Trim()
            : analysis.CompanyName;

        var job = new JobDescription
        {
            UserId = userId,
            JobTitle = finalJobTitle,
            CompanyName = finalCompany,
            RawContent = rawContent,
            ExperienceRequirement = analysis.ExperienceRequirement,
            RequiredSkillsJson = JsonSerializer.Serialize(analysis.RequiredSkills ?? new List<string>()),
            PreferredSkillsJson = JsonSerializer.Serialize(analysis.PreferredSkills ?? new List<string>()),
            ResponsibilitiesJson = JsonSerializer.Serialize(analysis.Responsibilities ?? new List<string>()),
            EducationRequirementsJson = JsonSerializer.Serialize(analysis.EducationRequirements ?? new List<string>()),
            KeywordsJson = JsonSerializer.Serialize(analysis.Keywords ?? new List<string>()),
            IsAnalyzed = true,
            AnalyzedAtUtc = DateTime.UtcNow
        };

        _dbContext.JobDescriptions.Add(job);

        // Record AI Usage audit
        _dbContext.UsageRecords.Add(new UsageRecord
        {
            UserId = userId,
            FeatureName = "JobAnalysis",
            Provider = _aiService.ProviderName,
            TokensUsed = 150,
            Details = $"Analyzed job circular for role '{finalJobTitle}'"
        });

        await _dbContext.SaveChangesAsync(ct);

        return Result<JobDescriptionDto>.Success(MapToDto(job), "Job circular analyzed successfully.");
    }

    public async Task<Result<JobDescriptionDto>> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var job = await _dbContext.JobDescriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct);

        if (job == null)
        {
            return Result<JobDescriptionDto>.Failure("Job circular not found or unauthorized access.");
        }

        return Result<JobDescriptionDto>.Success(MapToDto(job));
    }

    public async Task<Result<List<JobDescriptionSummaryDto>>> GetAllByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var jobs = await _dbContext.JobDescriptions
            .AsNoTracking()
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAtUtc)
            .ToListAsync(ct);

        var summaries = jobs.Select(j =>
        {
            var requiredCount = ParseJsonList(j.RequiredSkillsJson).Count;
            return new JobDescriptionSummaryDto(
                Id: j.Id,
                JobTitle: j.JobTitle,
                CompanyName: j.CompanyName,
                ExperienceRequirement: j.ExperienceRequirement,
                RequiredSkillsCount: requiredCount,
                CreatedAtUtc: j.CreatedAtUtc
            );
        }).ToList();

        return Result<List<JobDescriptionSummaryDto>>.Success(summaries);
    }

    public async Task<Result<JobDescriptionDto>> UpdateAsync(Guid userId, Guid id, UpdateJobDescriptionRequestDto request, CancellationToken ct = default)
    {
        var job = await _dbContext.JobDescriptions
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct);

        if (job == null)
        {
            return Result<JobDescriptionDto>.Failure("Job circular not found or unauthorized access.");
        }

        job.JobTitle = request.JobTitle.Trim();
        job.CompanyName = request.CompanyName?.Trim();
        job.ExperienceRequirement = request.ExperienceRequirement?.Trim();
        job.RequiredSkillsJson = JsonSerializer.Serialize(request.RequiredSkills ?? new List<string>());
        job.PreferredSkillsJson = JsonSerializer.Serialize(request.PreferredSkills ?? new List<string>());
        job.ResponsibilitiesJson = JsonSerializer.Serialize(request.Responsibilities ?? new List<string>());
        job.EducationRequirementsJson = JsonSerializer.Serialize(request.EducationRequirements ?? new List<string>());
        job.KeywordsJson = JsonSerializer.Serialize(request.Keywords ?? new List<string>());

        await _dbContext.SaveChangesAsync(ct);

        return Result<JobDescriptionDto>.Success(MapToDto(job), "Job requirements updated successfully.");
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var job = await _dbContext.JobDescriptions
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct);

        if (job == null)
        {
            return Result.Failure("Job circular not found or unauthorized access.");
        }

        _dbContext.JobDescriptions.Remove(job);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success("Job circular deleted successfully.");
    }

    public async Task<Result<JobDescriptionDto>> ReanalyzeAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var job = await _dbContext.JobDescriptions
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId, ct);

        if (job == null)
        {
            return Result<JobDescriptionDto>.Failure("Job circular not found or unauthorized access.");
        }

        var analysis = await _aiService.AnalyzeJobDescriptionAsync(job.RawContent, ct);

        job.ExperienceRequirement = analysis.ExperienceRequirement;
        job.RequiredSkillsJson = JsonSerializer.Serialize(analysis.RequiredSkills ?? new List<string>());
        job.PreferredSkillsJson = JsonSerializer.Serialize(analysis.PreferredSkills ?? new List<string>());
        job.ResponsibilitiesJson = JsonSerializer.Serialize(analysis.Responsibilities ?? new List<string>());
        job.EducationRequirementsJson = JsonSerializer.Serialize(analysis.EducationRequirements ?? new List<string>());
        job.KeywordsJson = JsonSerializer.Serialize(analysis.Keywords ?? new List<string>());
        job.IsAnalyzed = true;
        job.AnalyzedAtUtc = DateTime.UtcNow;

        _dbContext.UsageRecords.Add(new UsageRecord
        {
            UserId = userId,
            FeatureName = "JobAnalysis",
            Provider = _aiService.ProviderName,
            TokensUsed = 150,
            Details = $"Re-analyzed job circular for role '{job.JobTitle}'"
        });

        await _dbContext.SaveChangesAsync(ct);

        return Result<JobDescriptionDto>.Success(MapToDto(job), "Job circular re-analyzed successfully.");
    }

    private static JobDescriptionDto MapToDto(JobDescription j)
    {
        return new JobDescriptionDto(
            Id: j.Id,
            UserId: j.UserId,
            JobTitle: j.JobTitle,
            CompanyName: j.CompanyName,
            RawContent: j.RawContent,
            ExperienceRequirement: j.ExperienceRequirement,
            RequiredSkills: ParseJsonList(j.RequiredSkillsJson),
            PreferredSkills: ParseJsonList(j.PreferredSkillsJson),
            Responsibilities: ParseJsonList(j.ResponsibilitiesJson),
            EducationRequirements: ParseJsonList(j.EducationRequirementsJson),
            Keywords: ParseJsonList(j.KeywordsJson),
            IsAnalyzed: j.IsAnalyzed,
            AnalyzedAtUtc: j.AnalyzedAtUtc,
            CreatedAtUtc: j.CreatedAtUtc
        );
    }

    private static List<string> ParseJsonList(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]") return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
