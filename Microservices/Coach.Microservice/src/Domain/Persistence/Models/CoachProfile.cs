using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public partial class CoachProfile
{
    public Guid UserId { get; set; }

    public string? Bio { get; set; }

    public int? YearsExperience { get; set; }

    public long? BasePriceVnd { get; set; }

    public decimal? ServiceRadiusKm { get; set; }

    public string? KycNote { get; set; }

    public KycStatus KycStatus { get; set; }

    public decimal? RatingAvg { get; set; }

    public int RatingCount { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CoachCertification> CoachCertifications { get; set; } = new List<CoachCertification>();

    public virtual ICollection<CoachMedium> CoachMedia { get; set; } = new List<CoachMedium>();

    public virtual ICollection<CoachService> CoachServices { get; set; } = new List<CoachService>();

    public virtual ICollection<KycRecord> KycRecords { get; set; } = new List<KycRecord>();

    public virtual ICollection<Sport> Sports { get; set; } = new List<Sport>();
}
