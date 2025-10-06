using System;
using System.Collections.Generic;

namespace Domain.Persistence.Models;

public partial class CoachCertification
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string CertName { get; set; } = null!;

    public string? Issuer { get; set; }

    public DateOnly? IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public string? FileUrl { get; set; }

    public string Status { get; set; } = null!;

    public Guid? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CoachProfile Coach { get; set; } = null!;
}
