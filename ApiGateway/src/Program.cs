using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
const string AllowFrontendPolicy = "AllowFrontend";

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("UserPolicy", p => p.RequireAuthenticatedUser());
    o.AddPolicy("AdminOnly", p => p.RequireRole("admin"));
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var vercelProd = "https://fitverse-five.vercel.app";
var customDomains = new[] { "https://www.yourdomain.com", "https://yourdomain.com" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowFrontendPolicy, policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                var host = uri.Host;
                return host.Equals("localhost", System.StringComparison.OrdinalIgnoreCase)
                       || host.Equals("127.0.0.1", System.StringComparison.OrdinalIgnoreCase)
                       || host.EndsWith("vercel.app", System.StringComparison.OrdinalIgnoreCase)
                       || host.EndsWith("ngrok-free.app", System.StringComparison.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

var swaggerEndpoints = new (string Url, string Name)[]
{
    ("/api/auth/swagger/v1/swagger.json", "Auth Service v1"),
    ("/api/coach/swagger/v1/swagger.json", "Coach Service v1"),
    ("/api/payment/swagger/v1/swagger.json", "Payment Service v1"),
    ("/api/booking/swagger/v1/swagger.json", "Booking Service v1"),
    ("/api/engage/swagger/v1/swagger.json", "Engage Service v1")
};

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    foreach (var (url, name) in swaggerEndpoints)
    {
        c.SwaggerEndpoint(url, name);
    }

    c.RoutePrefix = "docs";
});

app.MapGet("/health", () => "ok");

app.UseCors(AllowFrontendPolicy);

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy().RequireCors(AllowFrontendPolicy);

app.Run("http://0.0.0.0:8080");

