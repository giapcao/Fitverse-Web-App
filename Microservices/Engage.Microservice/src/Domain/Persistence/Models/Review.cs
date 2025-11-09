using System;

namespace Domain.Persistence.Models;

public class Review
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid CoachId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }
}

