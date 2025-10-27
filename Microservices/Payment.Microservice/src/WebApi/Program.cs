using System;
using System.Reflection;
using Application;
using Application.Options;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using SharedLibrary.Common;
using SharedLibrary.Configs;
using SharedLibrary.Contracts.Payments;
using SharedLibrary.Utils;
using WebApi.Constants;
using WebApi.HostedServices;

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
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyNames.VNPay, policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment", Version = "v1" });
    
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


builder.Services.ConfigureOptions<DatabaseConfigSetup>();
builder.Services.AddSingleton(sp =>
{
    var databaseConfig = sp.GetRequiredService<IOptions<DatabaseConfig>>().Value;
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(databaseConfig.ConnectionString);

    dataSourceBuilder.MapEnum<Dc>("dc_enum");
    dataSourceBuilder.MapEnum<Gateway>("gateway_enum");
    dataSourceBuilder.MapEnum<PaymentStatus>("payment_status_enum");
    dataSourceBuilder.MapEnum<WalletAccountType>("wallet_account_type_enum");
    dataSourceBuilder.MapEnum<WalletJournalStatus>("wallet_journal_status_enum");
    dataSourceBuilder.MapEnum<WalletJournalType>("wallet_journal_type_enum");
    dataSourceBuilder.MapEnum<WalletStatus>("wallet_status_enum");

    return dataSourceBuilder.Build();
});
builder.Services.Configure<VNPayOptions>(builder.Configuration.GetSection(VNPayOptions.SectionName));
builder.Services.Configure<MomoOptions>(builder.Configuration.GetSection(MomoOptions.SectionName));
builder.Services.AddCompanyJwtAuth(builder.Configuration);
builder.Services
    .AddApplication()
    .AddInfrastructure();
builder.Services.AddHostedService<PaymentReturnTimeoutService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                $"Payment API {desc.GroupName.ToUpperInvariant()}");
        }
        c.SwaggerEndpoint("/swagger/vnpay/swagger.json", "VNPay API");

        c.RoutePrefix = "swagger";
    });

    app.MapGet("/", ctx =>
    {
        ctx.Response.Redirect("/swagger");
        return Task.CompletedTask;
    });
}

app.UseSwagger();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor,
    KnownNetworks = { }, 
    KnownProxies  = { }
});

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

AutoScaffold.UpdateAppSettingsFile("appsettings.json", "default");
AutoScaffold.UpdateAppSettingsFile("appsettings.Development.json", "default");
