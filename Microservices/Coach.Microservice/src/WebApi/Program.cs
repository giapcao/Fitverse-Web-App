using System.Reflection;
using System.Text.Json.Serialization;
using Application;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Domain.Persistence;
using Domain.Persistence.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using Options = Infrastructure.Common.Options;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment;

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration));

builder.Services
    .AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Coach", Version = "v1" });
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
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

builder.Services
    .AddOptions<Options.SmtpOptions>()
    .Bind(builder.Configuration.GetSection("Smtp"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "Smtp:Host is required.")
    .Validate(o => o.Port > 0, "Smtp:Port must be greater than 0.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.User), "Smtp:User is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.Pass), "Smtp:Pass is required.")
    .Validate(o => !string.IsNullOrWhiteSpace(o.FromName), "Smtp:FromName is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<Options.CoachAppOptions>()
    .Bind(builder.Configuration.GetSection("CoachApp"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.DashboardUrl), "CoachApp:DashboardUrl is required.")
    .ValidateOnStart();

builder.Services.ConfigureOptions<DatabaseConfigSetup>();
builder.Services.Configure<AwsS3Config>(builder.Configuration.GetSection("AwsS3"));
builder.Services.AddDbContext<FitverseCoachDbContext>((sp, options) =>
{
    var dbConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
    options.UseNpgsql(dbConfig.ConnectionString, npgsqlOptions =>
    {
        if (dbConfig.MaxRetryCount > 0)
        {
            npgsqlOptions.EnableRetryOnFailure(dbConfig.MaxRetryCount);
        }

        if (dbConfig.CommandTimeout > 0)
        {
            npgsqlOptions.CommandTimeout(dbConfig.CommandTimeout);
        }

        npgsqlOptions.MapEnum<KycStatus>("kyc_status_enum");
        npgsqlOptions.MapEnum<CoachMediaType>("media_type_enum");
    });

    if (environment.IsDevelopment())
    {
        options.EnableDetailedErrors(dbConfig.EnableDetailedErrors);
        options.EnableSensitiveDataLogging(dbConfig.EnableSensitiveDataLogging);
    }
});

builder.Services.AddCompanyJwtAuth(builder.Configuration);
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Health check endpoints
app.MapGet("/health", () => new { status = "ok" });
app.MapGet("/api/health", () => new { status = "ok" });

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(c =>
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"coach/swagger/{description.GroupName}/swagger.json", 
            $"Coach API {description.GroupName.ToUpperInvariant()}");
        }

        c.RoutePrefix = "swagger";
    });

    app.MapGet("/", ctx =>
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

