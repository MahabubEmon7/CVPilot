using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.JobDescriptions.DTOs;
using CVPilot.Application.Features.JobDescriptions.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVPilot.API.Controllers;

[Authorize]
public class JobDescriptionsController : BaseApiController
{
    private readonly IJobDescriptionService _jobService;
    private readonly ICurrentUserService _currentUserService;

    public JobDescriptionsController(IJobDescriptionService jobService, ICurrentUserService currentUserService)
    {
        _jobService = jobService;
        _currentUserService = currentUserService;
    }

    private Guid CurrentUserId => _currentUserService.UserId ?? throw new UnauthorizedAccessException();

    [HttpPost]
    public async Task<ActionResult<ApiResponse<JobDescriptionDto>>> CreateAndAnalyze(
        [FromBody] CreateJobDescriptionRequestDto request, 
        CancellationToken ct)
    {
        var result = await _jobService.CreateAndAnalyzeAsync(CurrentUserId, request, ct);
        return HandleResult(result);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobDescriptionSummaryDto>>>> GetAll(CancellationToken ct)
    {
        var result = await _jobService.GetAllByUserAsync(CurrentUserId, ct);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<JobDescriptionDto>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _jobService.GetByIdAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<JobDescriptionDto>>> Update(
        Guid id, 
        [FromBody] UpdateJobDescriptionRequestDto request, 
        CancellationToken ct)
    {
        var result = await _jobService.UpdateAsync(CurrentUserId, id, request, ct);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var result = await _jobService.DeleteAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }

    [HttpPost("{id}/reanalyze")]
    public async Task<ActionResult<ApiResponse<JobDescriptionDto>>> Reanalyze(Guid id, CancellationToken ct)
    {
        var result = await _jobService.ReanalyzeAsync(CurrentUserId, id, ct);
        return HandleResult(result);
    }
}
