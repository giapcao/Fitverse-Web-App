using System.Reflection;
using System.Text;
using Application;
using Application.Common;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Infrastructure.Context;
using Microsoft.OpenApi.Models;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using SharedLibrary.Utils;
using StackExchange.Redis;
using Options = Infrastructure.Common.Options;

string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "";
if (solutionDirectory != null)
{
    DotNetEnv.Env.Load(Path.Combine(solutionDirectory, ".env"));
}

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration));

builder.Services
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });


builder.Services.AddControllers();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth", Version = "v1" });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });


    var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, true);
});
builder.Services
    .AddOptions<Options.SmtpOptions>()
    .Bind(builder.Configuration.GetSection("Smtp"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "Smtp:Host is required.")
    .Validate(o => o.Port > 0, "Smtp:Port must be > 0.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.User), "Smtp:User is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Pass), "Smtp:Pass is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.FromName), "Smtp:FromAddress is required.")
    .ValidateOnStart();

builder.Services.Configure<OtpOptions>(builder.Configuration.GetSection("Otp"));
builder.Services
    .AddOptions<Options.GoogleOAuthOptions>()
    .Bind(builder.Configuration.GetSection("Google"));


static byte[] GetKeyBytes(string key)
{
    bool isHex = !string.IsNullOrWhiteSpace(key)
                 && key.All(Uri.IsHexDigit)
                 && key.Length % 2 == 0;
    return isHex ? Convert.FromHexString(key) : Encoding.UTF8.GetBytes(key);
}

builder.Services
    .AddOptions<Options.JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && GetKeyBytes(o.Key).Length >= 32,
        "Jwt:Key must be at least 32 bytes (after decode).")
    .ValidateOnStart();

builder.Services
    .AddOptions<Options.RefreshOptions>()
    .Bind(builder.Configuration.GetSection("Refresh"))
    .Validate(o => o.ExpiryDays > 0, "Refresh:ExpiryDays must be > 0")
    .ValidateOnStart();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
    var port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
    var pwd  = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

    var cfg = $"{host}:{port},abortConnect=false";
    if (!string.IsNullOrWhiteSpace(pwd))
        cfg += $",password={pwd}";

    return ConnectionMultiplexer.Connect(cfg);
});

// var jwt = builder.Configuration.GetSection("Jwt").Get<Options.JwtOptions>()!;
// var signingKey = new SymmetricSecurityKey(GetKeyBytes(jwt.Key));
//
// builder.Services.AddSingleton(signingKey);
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
//     {
//         o.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidIssuer = jwt.Issuer,
//             ValidAudience = jwt.Audience,
//             IssuerSigningKey = signingKey,
//             ValidateIssuerSigningKey = true,
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ClockSkew = TimeSpan.Zero
//         };
//     });



builder.Services.ConfigureOptions<DatabaseConfigSetup>();
builder.Services.AddDbContext<FitverseDbContext>((sp, options) =>
{
    var dbCfg = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
    options.UseNpgsql(dbCfg.ConnectionString, actions =>
    {
        actions.EnableRetryOnFailure(dbCfg.MaxRetryCount);
        actions.CommandTimeout(dbCfg.CommandTimeout);
    });
    if (environment.IsDevelopment())
    {
        options.EnableDetailedErrors(dbCfg.EnableDetailedErrors);
        options.EnableSensitiveDataLogging(dbCfg.EnableSensitiveDataLogging);
    }
});

builder.Services.AddCompanyJwtAuth(builder.Configuration);
builder.Services
    .AddApplication()
    .AddInfrastructure();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"auth/swagger/{desc.GroupName}/swagger.json",
                $"API {desc.GroupName.ToUpperInvariant()}");
        }

        c.RoutePrefix = "swagger";
    });

    app.MapGet("/", ctx =>
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseSwagger();


app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

AutoScaffold.UpdateAppSettingsFile("appsettings.json", "default");
AutoScaffold.UpdateAppSettingsFile("appsettings.Development.json", "default");
