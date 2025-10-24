using System;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Contracts.Requests;

public sealed class CreateCoachCertificationRequest
{
    public Guid CoachId { get; init; }
    public string CertName { get; init; } = string.Empty;
    public string? Issuer { get; init; }
    public DateOnly? IssuedOn { get; init; }
    public DateOnly? ExpiresOn { get; init; }
    public string? FileUrl { get; init; }
    public string? Directory { get; init; } = "certifications";
    public IFormFile? File { get; init; }
}
