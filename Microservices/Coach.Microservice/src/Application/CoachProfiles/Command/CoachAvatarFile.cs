namespace Application.CoachProfiles.Command;

public sealed record CoachAvatarFile(
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = null);
