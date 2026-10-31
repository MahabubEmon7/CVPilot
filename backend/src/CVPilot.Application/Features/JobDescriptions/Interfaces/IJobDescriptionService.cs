using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.JobDescriptions.DTOs;

namespace CVPilot.Application.Features.JobDescriptions.Interfaces;

public interface IJobDescriptionService
{
    Task<Result<JobDescriptionDto>> CreateAndAnalyzeAsync(Guid userId, CreateJobDescriptionRequestDto request, CancellationToken ct = default);
    Task<Result<JobDescriptionDto>> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<Result<List<JobDescriptionSummaryDto>>> GetAllByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<JobDescriptionDto>> UpdateAsync(Guid userId, Guid id, UpdateJobDescriptionRequestDto request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<Result<JobDescriptionDto>> ReanalyzeAsync(Guid userId, Guid id, CancellationToken ct = default);
}
