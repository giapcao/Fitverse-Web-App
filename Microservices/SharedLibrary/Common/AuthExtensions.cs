using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SharedLibrary.Common;

public static class AuthExtensions
{
    public static IServiceCollection AddCompanyJwtAuth(
        this IServiceCollection services, IConfiguration config)
    {
        var auth = config.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(GetKeyBytes(auth["Key"]));

        services.AddSingleton(signingKey);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = auth["Authority"];
                o.Audience = auth["Audience"];
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = auth["RoleClaim"] ?? ClaimTypes.Role,
                    NameClaimType = "sub",
                    ValidIssuer = auth["Issuer"],
                    ValidAudience = auth["Audience"],
                    IssuerSigningKey = signingKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("IsCustomer", p => p.RequireRole("Customer"));
            options.AddPolicy("IsCoach", p => p.RequireRole("Coach"));
            options.AddPolicy("IsAdmin", p => p.RequireRole("Admin"));
            options.AddPolicy("IsSupport", p => p.RequireRole("Support"));
        });

        return services;
    }

    static byte[] GetKeyBytes(string? key)
    {
        bool isHex = !string.IsNullOrWhiteSpace(key)
                     && key.All(Uri.IsHexDigit)
                     && key.Length % 2 == 0;
        return isHex ? Convert.FromHexString(key) : Encoding.UTF8.GetBytes(key);
    }
}