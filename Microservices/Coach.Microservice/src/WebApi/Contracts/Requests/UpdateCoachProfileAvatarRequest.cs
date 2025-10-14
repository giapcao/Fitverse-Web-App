using System;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Contracts.Requests;

public sealed class UpdateCoachProfileAvatarRequest
{
    public Guid CoachId { get; init; }
    public string? Directory { get; init; } = "avatar";
    public IFormFile? File { get; init; }
}
