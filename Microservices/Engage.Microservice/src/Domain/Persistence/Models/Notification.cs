using System;
using Domain.Persistence.Enums;

namespace Domain.Persistence.Models;

public class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationChannel Channel { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? Data { get; set; }

    public DateTime SentAt { get; set; }

    public Guid? CampaignId { get; set; }

    public virtual NotificationCampaign? Campaign { get; set; }
}

