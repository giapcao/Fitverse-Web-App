namespace Application.CoachMedia.Command;

public sealed record CoachMediaFile(
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = "media");
