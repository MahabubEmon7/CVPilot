namespace CVPilot.Application.Features.Auth.DTOs;

public record RegisterRequestDto(
    string FullName,
    string Email,
    string Password
);

public record LoginRequestDto(
    string Email,
    string Password
);

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    string Role
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInMinutes,
    UserDto User
);

