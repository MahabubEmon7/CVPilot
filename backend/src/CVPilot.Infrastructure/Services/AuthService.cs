using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Auth.DTOs;
using CVPilot.Application.Features.Auth.Interfaces;
using CVPilot.Domain.Entities;
using CVPilot.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CVPilot.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == normalizedEmail, ct);

        if (existingUser)
        {
            return Result<AuthResponseDto>.Failure("An account with this email address already exists.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new ApplicationUser
        {
            Email = normalizedEmail,
            FullName = request.FullName.Trim(),
            PasswordHash = passwordHash,
            Role = UserRole.User,
            IsActive = true
        };

        // Initialize user with empty master profile
        var profile = new UserProfile
        {
            User = user,
            Headline = "Software Professional",
            LocationCountry = "Bangladesh"
        };
        user.Profile = profile;

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryUtc = DateTime.UtcNow.AddDays(7);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString());
        var response = new AuthResponseDto(accessToken, refreshToken, 120, userDto);

        return Result<AuthResponseDto>.Success(response, "Registration successful.");
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, ct);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("This account has been deactivated. Please contact support.");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryUtc = DateTime.UtcNow.AddDays(7);
        await _dbContext.SaveChangesAsync(ct);

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString());
        var response = new AuthResponseDto(accessToken, refreshToken, 120, userDto);

        return Result<AuthResponseDto>.Success(response, "Login successful.");
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
        {
            return Result<UserDto>.Failure("User not found.");
        }

        var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString());
        return Result<UserDto>.Success(userDto);
    }
}

