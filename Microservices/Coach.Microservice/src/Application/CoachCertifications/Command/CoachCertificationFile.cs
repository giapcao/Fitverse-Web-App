namespace Application.CoachCertifications.Command;

public sealed record CoachCertificationFile(
    byte[] Content,
    string FileName,
    string ContentType,
    string? Directory = null);
