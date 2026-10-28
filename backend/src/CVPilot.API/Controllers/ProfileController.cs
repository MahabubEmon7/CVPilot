using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Profile.DTOs;
using CVPilot.Application.Features.Profile.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVPilot.API.Controllers;

[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;
    private readonly IResumeImportService _resumeImportService;
    private readonly ICurrentUserService _currentUserService;

    public ProfileController(
        IProfileService profileService,
        IResumeImportService resumeImportService,
        ICurrentUserService currentUserService)
    {
        _profileService = profileService;
        _resumeImportService = resumeImportService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedAccessException();

    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile(CancellationToken ct)
    {
        var result = await _profileService.GetProfileByUserIdAsync(CurrentUserId, ct);
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateProfile([FromBody] UpdateProfileRequestDto request, CancellationToken ct)
    {
        var result = await _profileService.UpdateProfileAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpPost("experience")]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> AddExperience([FromBody] CreateWorkExperienceDto request, CancellationToken ct)
    {
        var result = await _profileService.AddWorkExperienceAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpPut("experience/{id}")]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> UpdateExperience(Guid id, [FromBody] CreateWorkExperienceDto request, CancellationToken ct)
    {
        var result = await _profileService.UpdateWorkExperienceAsync(CurrentUserId, id, request, ct);
        return HandleResult(result);
    }

    [HttpDelete("experience/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteExperience(Guid id, CancellationToken ct)
    {
        var result = await _profileService.DeleteWorkExperienceAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPost("education")]
    public async Task<ActionResult<ApiResponse<EducationDto>>> AddEducation([FromBody] CreateEducationDto request, CancellationToken ct)
    {
        var result = await _profileService.AddEducationAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpPut("education/{id}")]
    public async Task<ActionResult<ApiResponse<EducationDto>>> UpdateEducation(Guid id, [FromBody] CreateEducationDto request, CancellationToken ct)
    {
        var result = await _profileService.UpdateEducationAsync(CurrentUserId, id, request, ct);
        return HandleResult(result);
    }

    [HttpDelete("education/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEducation(Guid id, CancellationToken ct)
    {
        var result = await _profileService.DeleteEducationAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPost("skills")]
    public async Task<ActionResult<ApiResponse<UserSkillDto>>> AddSkill([FromBody] CreateUserSkillDto request, CancellationToken ct)
    {
        var result = await _profileService.AddSkillAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpDelete("skills/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteSkill(Guid id, CancellationToken ct)
    {
        var result = await _profileService.DeleteSkillAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPost("projects")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> AddProject([FromBody] CreateProjectDto request, CancellationToken ct)
    {
        var result = await _profileService.AddProjectAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpPut("projects/{id}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] CreateProjectDto request, CancellationToken ct)
    {
        var result = await _profileService.UpdateProjectAsync(CurrentUserId, id, request, ct);
        return HandleResult(result);
    }

    [HttpDelete("projects/{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteProject(Guid id, CancellationToken ct)
    {
        var result = await _profileService.DeleteProjectAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPost("import/upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ParsedResumeDto>>> UploadResumeForParsing(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ParsedResumeDto>.Fail("No file was uploaded."));
        }

        const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB limit
        if (file.Length > maxSizeBytes)
        {
            return BadRequest(ApiResponse<ParsedResumeDto>.Fail("File size exceeds 5MB limit."));
        }

        await using var stream = file.OpenReadStream();
        var result = await _resumeImportService.ExtractResumeDataAsync(stream, file.FileName, ct);
        return HandleResult(result);
    }

    [HttpPost("import/confirm")]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> ConfirmImport([FromBody] ConfirmImportProfileDto dto, CancellationToken ct)
    {
        var result = await _resumeImportService.ConfirmAndImportToProfileAsync(CurrentUserId, dto, ct);
        return HandleResult(result);
    }
}

