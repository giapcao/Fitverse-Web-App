using System;
using Microsoft.AspNetCore.Http;

namespace WebApi.Contracts.Requests;

public sealed class UpdateUserAvatarRequest
{
    public Guid UserId { get; init; }
    public string? Directory { get; init; } = "user-avatar";
    public IFormFile? File { get; init; }
}
