using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public partial class FitverseEngageDbContext : DbContext
{
    public FitverseEngageDbContext()
    {
    }

    public FitverseEngageDbContext(DbContextOptions<FitverseEngageDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<DisputeTicket> DisputeTickets { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationCampaign> NotificationCampaigns { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<CampaignStatus>("campaign_status_enum")
            .HasPostgresEnum<DisputeStatus>("dispute_status_enum")
            .HasPostgresEnum<NotificationChannel>("notification_channel_enum")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("conversation_pkey");

            entity.ToTable("conversation");

            entity.HasIndex(e => new { e.UserId, e.CoachId }, "conversation_user_id_coach_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<DisputeTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("dispute_ticket_pkey");

            entity.ToTable("dispute_ticket");

            entity.HasIndex(e => e.BookingId, "dispute_ticket_booking_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EvidenceUrls)
                .HasColumnType("text[]")
                .HasColumnName("evidence_urls");
            entity.Property(e => e.OpenedBy).HasColumnName("opened_by");
            entity.Property(e => e.ReasonType).HasColumnName("reason_type");
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.ResolvedBy).HasColumnName("resolved_by");
            entity.Property(e => e.Status)
                .HasColumnType("dispute_status_enum")
                .HasDefaultValueSql("'open'::dispute_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("message_pkey");

            entity.ToTable("message");

            entity.HasIndex(e => new { e.ConversationId, e.CreatedAt }, "idx_message_conv");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AttachmentUrl).HasColumnName("attachment_url");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("message_conversation_id_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_pkey");

            entity.ToTable("notification");

            entity.HasIndex(e => new { e.UserId, e.SentAt }, "idx_notification_user").IsDescending(false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CampaignId).HasColumnName("campaign_id");
            entity.Property(e => e.Channel)
                .HasColumnType("notification_channel_enum")
                .HasColumnName("channel");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("sent_at");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notification_campaign_id_fkey");
        });

        modelBuilder.Entity<NotificationCampaign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notification_campaign_pkey");

            entity.ToTable("notification_campaign");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Audience).HasColumnName("audience");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Data)
                .HasColumnType("jsonb")
                .HasColumnName("data");
            entity.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
            entity.Property(e => e.Status)
                .HasColumnType("campaign_status_enum")
                .HasDefaultValueSql("'draft'::campaign_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.TemplateKey).HasColumnName("template_key");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("review_pkey");

            entity.ToTable("review");

            entity.HasIndex(e => e.CoachId, "idx_review_coach");

            entity.HasIndex(e => e.UserId, "idx_review_user");

            entity.HasIndex(e => e.BookingId, "review_booking_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPublic)
                .HasDefaultValue(true)
                .HasColumnName("is_public");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

