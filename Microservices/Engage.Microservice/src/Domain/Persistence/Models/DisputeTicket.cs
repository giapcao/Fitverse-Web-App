using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public class DisputeTicket
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid OpenedBy { get; set; }

    public DisputeStatus Status { get; set; }

    public string? ReasonType { get; set; }

    public string? Description { get; set; }

    public List<string>? EvidenceUrls { get; set; }

    public Guid? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

