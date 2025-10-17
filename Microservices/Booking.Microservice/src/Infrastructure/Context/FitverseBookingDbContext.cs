using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence;

public partial class FitverseBookingDbContext : DbContext
{
    public FitverseBookingDbContext()
    {
    }

    public FitverseBookingDbContext(DbContextOptions<FitverseBookingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AvailabilityRule> AvailabilityRules { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<CoachTimeoff> CoachTimeoffs { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionEvent> SubscriptionEvents { get; set; }

    public virtual DbSet<Timeslot> Timeslots { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<BookingStatus>("public", "booking_status_enum")
            .HasPostgresEnum<SlotStatus>("public", "slot_status_enum")
            .HasPostgresEnum<SubscriptionEventType>("public", "subscription_event_type_enum")
            .HasPostgresEnum<SubscriptionStatus>("public", "subscription_status_enum")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<AvailabilityRule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("availability_rule_pkey");

            entity.ToTable("availability_rule");

            entity.HasIndex(e => e.CoachId, "idx_availability_rule_coach");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsOnline)
                .HasDefaultValue(true)
                .HasColumnName("is_online");
            entity.Property(e => e.OnsiteLat).HasColumnName("onsite_lat");
            entity.Property(e => e.OnsiteLng).HasColumnName("onsite_lng");
            entity.Property(e => e.SlotDurationMinutes)
                .HasDefaultValue(60)
                .HasColumnName("slot_duration_minutes");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Timezone)
                .HasDefaultValueSql("'Asia/Ho_Chi_Minh'::text")
                .HasColumnName("timezone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Weekday).HasColumnName("weekday");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("booking_pkey");

            entity.ToTable("booking");

            entity.HasIndex(e => new { e.CoachId, e.StartAt }, "idx_booking_coach");

            entity.HasIndex(e => new { e.UserId, e.StartAt }, "idx_booking_user");

            entity.HasIndex(e => e.Status, "idx_booking_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CommissionPct)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("15.00")
                .HasColumnName("commission_pct");
            entity.Property(e => e.CommissionVnd).HasColumnName("commission_vnd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasDefaultValueSql("'VND'::text")
                .HasColumnName("currency_code");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.Status)
                .HasColumnType("booking_status_enum")
                .HasDefaultValueSql("'pending_payment'::booking_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.GrossAmountVnd).HasColumnName("gross_amount_vnd");
            entity.Property(e => e.LocationNote).HasColumnName("location_note");
            entity.Property(e => e.NetAmountVnd).HasColumnName("net_amount_vnd");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceTitle).HasColumnName("service_title");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.TimeslotId).HasColumnName("timeslot_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Timeslot).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.TimeslotId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_booking_timeslot");
        });

        modelBuilder.Entity<CoachTimeoff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coach_timeoff_pkey");

            entity.ToTable("coach_timeoff");

            entity.HasIndex(e => e.CoachId, "idx_timeoff_coach");

            entity.HasIndex(e => new { e.CoachId, e.StartAt, e.EndAt }, "idx_timeoff_range");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_pkey");

            entity.ToTable("subscription");

            entity.HasIndex(e => new { e.CoachId, e.PeriodStart }, "idx_subscription_coach");

            entity.HasIndex(e => new { e.UserId, e.PeriodStart }, "idx_subscription_user");

            entity.HasIndex(e => new { e.UserId, e.CoachId, e.ServiceId, e.PeriodStart, e.PeriodEnd }, "ux_subscription_unique_period").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CommissionPct)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("15.00")
                .HasColumnName("commission_pct");
            entity.Property(e => e.CommissionVnd)
                .HasDefaultValue(0L)
                .HasColumnName("commission_vnd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrencyCode)
                .HasDefaultValueSql("'VND'::text")
                .HasColumnName("currency_code");
            entity.Property(e => e.Status)
                .HasColumnType("subscription_status_enum")
                .HasDefaultValueSql("'active'::subscription_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.NetAmountVnd)
                .HasDefaultValue(0L)
                .HasColumnName("net_amount_vnd");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PriceGrossVnd).HasColumnName("price_gross_vnd");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.SessionsConsumed)
                .HasDefaultValue(0)
                .HasColumnName("sessions_consumed");
            entity.Property(e => e.SessionsReserved)
                .HasDefaultValue(0)
                .HasColumnName("sessions_reserved");
            entity.Property(e => e.SessionsTotal).HasColumnName("sessions_total");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<SubscriptionEvent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscription_event_pkey");

            entity.ToTable("subscription_event");

            entity.HasIndex(e => new { e.SubscriptionId, e.CreatedAt }, "idx_subscription_event_sub");

            entity.HasIndex(e => e.EventType, "idx_subscription_event_type");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EventType)
                .HasColumnType("subscription_event_type_enum")
                .HasColumnName("event_type");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.TimeslotId).HasColumnName("timeslot_id");

            entity.HasOne(d => d.Booking).WithMany(p => p.SubscriptionEvents)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_subscription_event_booking");

            entity.HasOne(d => d.Subscription).WithMany(p => p.SubscriptionEvents)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("subscription_event_subscription_id_fkey");

            entity.HasOne(d => d.Timeslot).WithMany(p => p.SubscriptionEvents)
                .HasForeignKey(d => d.TimeslotId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_subscription_event_timeslot");
        });

        modelBuilder.Entity<Timeslot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timeslot_pkey");

            entity.ToTable("timeslot");

            entity.HasIndex(e => new { e.CoachId, e.StartAt }, "idx_timeslot_user_time");

            entity.HasIndex(e => new { e.CoachId, e.StartAt, e.EndAt }, "timeslot_coach_id_start_at_end_at_key").IsUnique();

            entity.HasIndex(e => e.Status, "idx_timeslot_status");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Capacity)
                .HasDefaultValue(1)
                .HasColumnName("capacity");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.Status)
                .HasColumnType("slot_status_enum")
                .HasDefaultValueSql("'open'::slot_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.IsOnline)
                .HasDefaultValue(true)
                .HasColumnName("is_online");
            entity.Property(e => e.OnsiteLat).HasColumnName("onsite_lat");
            entity.Property(e => e.OnsiteLng).HasColumnName("onsite_lng");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
