using System;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Contracts.Requests;

public sealed class UpdateCoachCertificationRequest
{
    public string? CertName { get; init; }
    public string? Issuer { get; init; }
    public DateOnly? IssuedOn { get; init; }
    public DateOnly? ExpiresOn { get; init; }
    public string? FileUrl { get; init; }
    public string? Directory { get; init; } = "certifications";
    public IFormFile? File { get; init; }
}
