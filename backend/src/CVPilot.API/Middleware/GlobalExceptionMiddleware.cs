using System.Net;
using System.Text.Json;
using CVPilot.Application.Common.Exceptions;
using CVPilot.Application.Common.Models;

namespace CVPilot.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponse { Success = false };

        switch (exception)
        {
            case ValidationException valEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = valEx.Message;
                foreach (var err in valEx.Errors)
                {
                    foreach (var msg in err.Value)
                    {
                        response.Errors.Add($"{err.Key}: {msg}");
                    }
                }
                _logger.LogWarning("Validation exception: {Message}", valEx.Message);
                break;

            case NotFoundException notFoundEx:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = notFoundEx.Message;
                response.Errors.Add(notFoundEx.Message);
                _logger.LogInformation("Not found: {Message}", notFoundEx.Message);
                break;

            case UnauthorizedException unauthEx:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response.Message = unauthEx.Message;
                response.Errors.Add(unauthEx.Message);
                break;

            case ForbiddenException forbEx:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = forbEx.Message;
                response.Errors.Add(forbEx.Message);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred. Please try again later.";
                if (_env.IsDevelopment())
                {
                    response.Errors.Add(exception.Message);
                    if (exception.StackTrace != null)
                    {
                        response.Errors.Add(exception.StackTrace);
                    }
                }
                else
                {
                    response.Errors.Add("Server error occurred.");
                }
                _logger.LogError(exception, "Unhandled error: {Message}", exception.Message);
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

