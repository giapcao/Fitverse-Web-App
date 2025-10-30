using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

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
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return true;
                if (origin.Equals("http://localhost:5173", StringComparison.OrdinalIgnoreCase)) return true;
                if (origin.Equals(vercelProd, StringComparison.OrdinalIgnoreCase)) return true;

                try
                {
                    var uri = new Uri(origin);
                    var host = uri.Host;

                    if (customDomains.Any(d => new Uri(d).Host.Equals(host, StringComparison.OrdinalIgnoreCase)))
                        return true;

                    if (host.Equals("https://fitverse-five.vercel.app", StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (host.EndsWith("-your-app.vercel.app", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch {}

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/auth/swagger/v1/swagger.json", "Auth Service v1");
    c.SwaggerEndpoint("/api/coach/swagger/v1/swagger.json", "Coach Service v1");
    c.SwaggerEndpoint("/api/payment/swagger/v1/swagger.json", "Payment Service v1");
    c.SwaggerEndpoint("/api/booking/swagger/v1/swagger.json", "Booking Service v1");
    c.RoutePrefix = "docs";
});

app.MapGet("/health", () => "ok");

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy().RequireCors("AllowFrontend");

app.Run("http://0.0.0.0:8080");
