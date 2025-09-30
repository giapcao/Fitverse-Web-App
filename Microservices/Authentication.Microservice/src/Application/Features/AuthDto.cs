using System.Collections.Generic;

namespace Application.Features;

public class AuthDto
{
    public sealed record TokenPairDto(string AccessToken, string RefreshToken);
    public sealed record LoginResultDto(
        Guid UserId,
        string Email,
        string FullName,
        IEnumerable<RoleDto> Roles,
        TokenPairDto Tokens);

    public sealed record VerifyEmailResultDto(bool Success);

    public sealed record GetMeDto(Guid UserId, string Email, string FullName, IEnumerable<RoleDto> Roles);
    
    public sealed class RequestOtpDto { public string Email { get; set; } = default!; }
    public sealed class VerifyOtpDto 
    {
        public string Email { get; set; } = default!;
        public string Otp { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
    
    public sealed class VerifyOtpOkDto
    {
        public string ResetToken { get; set; } = default!;
        public int ExpiresInSeconds { get; set; }
    }
    
    public sealed class VerifyOtpOnlyDto
    {
        public string Email { get; set; } = default!;
        public string Otp { get; set; } = default!;
    }

    public sealed class ResetWithTokenDto
    {
        public string ResetToken { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }
}