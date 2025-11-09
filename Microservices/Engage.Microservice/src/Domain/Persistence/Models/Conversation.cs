using System;
using System.Collections.Generic;

namespace Domain.Persistence.Models;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid CoachId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

