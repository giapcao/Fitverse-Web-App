using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public partial class KycRecord
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string? IdDocumentUrl { get; set; }

    public string? AdminNote { get; set; }

    public KycStatus Status { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewerId { get; set; }

    public virtual CoachProfile Coach { get; set; } = null!;
}
