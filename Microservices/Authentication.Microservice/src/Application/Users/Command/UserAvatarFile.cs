namespace Application.Users.Command;

public sealed record UserAvatarFile(
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = null);
