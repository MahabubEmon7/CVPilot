using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Profile.DTOs;

namespace CVPilot.Application.Features.Profile.Interfaces;

public interface IResumeImportService
{
    Task<Result<ParsedResumeDto>> ExtractResumeDataAsync(Stream fileStream, string fileName, CancellationToken ct = default);
    Task<Result<UserProfileDto>> ConfirmAndImportToProfileAsync(Guid userId, ConfirmImportProfileDto dto, CancellationToken ct = default);
}

