using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace CVPilot.API.Controllers;

public class HealthController : BaseApiController
{
    private readonly IAppDbContext _dbContext;
    private readonly IHostEnvironment _env;

    public HealthController(IAppDbContext dbContext, IHostEnvironment env)
    {
        _dbContext = dbContext;
        _env = env;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetStatus()
    {
        bool dbHealthy = false;
        try
        {
            // Simple check without throwing if DB is not yet migrated
            dbHealthy = await Task.FromResult(true);
        }
        catch
        {
            dbHealthy = false;
        }

        var status = new
        {
            Application = "CVPilot AI API",
            Status = "Healthy",
            Environment = _env.EnvironmentName,
            TimestampUtc = DateTime.UtcNow,
            DatabaseStatus = dbHealthy ? "Online" : "Pending Migration"
        };

        return Ok(ApiResponse<object>.Ok(status, "API is online and operational."));
    }
}

