using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public class NotificationCampaign
{
    public Guid Id { get; set; }

    public string Audience { get; set; } = null!;

    public string? TemplateKey { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? Data { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public CampaignStatus Status { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

