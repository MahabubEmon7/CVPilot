using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Auth.DTOs;

namespace CVPilot.Application.Features.Auth.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default);
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}

