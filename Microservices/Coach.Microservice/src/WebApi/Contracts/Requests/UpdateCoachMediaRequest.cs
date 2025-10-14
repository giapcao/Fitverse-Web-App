using System;
using Domain.Persistence.Enums;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Contracts.Requests;

public sealed class UpdateCoachMediaRequest
{
    public string? MediaName { get; init; }
    public string? Description { get; init; }
    public CoachMediaType? MediaType { get; init; }
    public string? Url { get; init; }
    public bool? IsFeatured { get; init; }
    public string? Directory { get; init; } = "media";
    public IFormFile? File { get; init; }
}
