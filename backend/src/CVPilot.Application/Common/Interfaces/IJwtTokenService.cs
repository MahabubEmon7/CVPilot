using CVPilot.Domain.Entities;

namespace CVPilot.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    Guid? ValidateTokenAndGetUserId(string token);
}

