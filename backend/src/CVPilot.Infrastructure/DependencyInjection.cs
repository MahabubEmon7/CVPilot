using System.Text;
using CVPilot.Application.Common.Interfaces;
using CVPilot.Infrastructure.Ai;
using CVPilot.Infrastructure.Auth;
using CVPilot.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CVPilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=CVPilotDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        // Security / Auth
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<CVPilot.Application.Features.Auth.Interfaces.IAuthService, CVPilot.Infrastructure.Services.AuthService>();
        services.AddScoped<CVPilot.Application.Features.Profile.Interfaces.IProfileService, CVPilot.Infrastructure.Services.ProfileService>();
        services.AddScoped<CVPilot.Application.Features.Profile.Interfaces.IResumeImportService, CVPilot.Infrastructure.Services.ResumeImportService>();

        // AI Provider (defaults to MockAiResumeService for development and testing)
        services.AddScoped<IAiResumeService, MockAiResumeService>();

        // JWT Authentication Configuration
        var secretKey = configuration["Jwt:SecretKey"] ?? "CVPilot_Super_Secret_Key_Default_Change_In_Production_2026";
        var issuer = configuration["Jwt:Issuer"] ?? "CVPilotAI";
        var audience = configuration["Jwt:Audience"] ?? "CVPilotAI-Clients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}

