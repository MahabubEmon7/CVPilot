using CVPilot.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace CVPilot.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> HandleResult<T>(Result<T> result)
    {
        if (result.Succeeded)
        {
            return Ok(ApiResponse<T>.Ok(result.Data!, result.Message));
        }

        return BadRequest(ApiResponse<T>.Fail(result.Errors));
    }

    protected ActionResult<ApiResponse> HandleResult(Result result)
    {
        if (result.Succeeded)
        {
            return Ok(ApiResponse.Ok(result.Message));
        }

        return BadRequest(ApiResponse.Fail(result.Errors));
    }
}

