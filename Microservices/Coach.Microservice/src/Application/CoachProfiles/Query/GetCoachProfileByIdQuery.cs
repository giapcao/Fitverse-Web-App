using System;
using Application.Abstractions.Messaging;
using Application.Features;

namespace Application.CoachProfiles.Query;

public sealed record GetCoachProfileByIdQuery(Guid CoachId) : IQuery<CoachProfileDto>;
