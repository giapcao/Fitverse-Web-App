using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public partial class CoachMedium
{
    public Guid Id { get; set; }

    public Guid CoachId { get; set; }

    public string? MediaName { get; set; }

    public CoachMediaType MediaType { get; set; }

    public string Url { get; set; } = null!;

    public bool Status { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual CoachProfile Coach { get; set; } = null!;
}
