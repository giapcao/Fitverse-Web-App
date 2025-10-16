using Application.Features;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Mapster;

namespace Application.Common.Mapper;

public static class MappingConfig
{
    public static void RegisterMappings(TypeAdapterConfig config)
    {
        config.NewConfig<AvailabilityRule, AvailabilityRuleDto>();

        config.NewConfig<Booking, BookingDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Timeslot, src => src.Timeslot)
            .Map(dest => dest.SubscriptionEvents, src => src.SubscriptionEvents);

        config.NewConfig<Timeslot, TimeslotSummaryDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());

        config.NewConfig<SubscriptionEvent, SubscriptionEventSummaryDto>()
            .Map(dest => dest.EventType, src => src.EventType.ToString());

        config.NewConfig<CoachTimeoff, CoachTimeoffDto>();

        config.NewConfig<Subscription, SubscriptionDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Events, src => src.SubscriptionEvents);

        config.NewConfig<SubscriptionEvent, SubscriptionEventDto>()
            .Map(dest => dest.EventType, src => src.EventType.ToString());

        config.NewConfig<Timeslot, TimeslotDto>()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Bookings, src => src.Bookings)
            .Map(dest => dest.SubscriptionEvents, src => src.SubscriptionEvents);

        config.NewConfig<Booking, BookingSummaryDto>()
            .Map(dest => dest.Status, src => src.Status.ToString());
    }
}
