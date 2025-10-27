using System;
using System.Collections.Generic;
using Domain.Persistence.Enums;
using Domain.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Persistence;

public partial class FitverseCoachDbContext : DbContext
{
    public FitverseCoachDbContext()
    {
    }

    public FitverseCoachDbContext(DbContextOptions<FitverseCoachDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CoachCertification> CoachCertifications { get; set; }

    public virtual DbSet<CoachMedium> CoachMedia { get; set; }

    public virtual DbSet<CoachProfile> CoachProfiles { get; set; }

    public virtual DbSet<CoachService> CoachServices { get; set; }

    public virtual DbSet<KycRecord> KycRecords { get; set; }

    public virtual DbSet<Sport> Sports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<KycStatus>("public", "kyc_status_enum")
            .HasPostgresEnum<CoachMediaType>("public", "media_type_enum")
            .HasPostgresExtension("citext")
            .HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<CoachCertification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coach_certification_pkey");

            entity.ToTable("coach_certification");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CertName).HasColumnName("cert_name");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.ExpiresOn).HasColumnName("expires_on");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.IssuedOn).HasColumnName("issued_on");
            entity.Property(e => e.Issuer).HasColumnName("issuer");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'active'::text")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Coach).WithMany(p => p.CoachCertifications)
                .HasForeignKey(d => d.CoachId)
                .HasConstraintName("fk_coach_cert_profile");
        });

        modelBuilder.Entity<CoachMedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coach_media_pkey");

            entity.ToTable("coach_media");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsFeatured)
                .HasDefaultValue(false)
                .HasColumnName("is_featured");
            entity.Property(e => e.MediaName).HasColumnName("media_name");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MediaType)
                .HasColumnType("media_type_enum")
                .HasColumnName("media_type");
            entity.Property(e => e.Status)
                .HasDefaultValue(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(d => d.Coach).WithMany(p => p.CoachMedia)
                .HasForeignKey(d => d.CoachId)
                .HasConstraintName("fk_coach_media_profile");
        });

        modelBuilder.Entity<CoachProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("coach_profile_pkey");

            entity.ToTable("coach_profile");

            entity.HasIndex(e => new { e.RatingAvg, e.RatingCount }, "idx_coach_profile_rating").IsDescending();

            entity.HasIndex(e => e.CitizenId, "uq_coach_profile_citizen_id")
                .IsUnique()
                .HasFilter("(citizen_id IS NOT NULL)");

            entity.HasIndex(e => e.TaxCode, "uq_coach_profile_tax_code")
                .IsUnique()
                .HasFilter("(tax_code IS NOT NULL)");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.AvatarUrl)
                .HasComment("URL ảnh đại diện của huấn luyện viên")
                .HasColumnName("avatar_url");
            entity.Property(e => e.BasePriceVnd).HasColumnName("base_price_vnd");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.BirthDate)
                .HasComment("Ngày sinh (DATE)")
                .HasColumnName("birth_date");
            entity.Property(e => e.CitizenId)
                .HasMaxLength(12)
                .HasComment("CCCD/CMND (9 hoặc 12 chữ số)")
                .HasColumnName("citizen_id");
            entity.Property(e => e.CitizenIssueDate)
                .HasComment("Ngày cấp CCCD/CMND")
                .HasColumnName("citizen_issue_date");
            entity.Property(e => e.CitizenIssuePlace)
                .HasComment("Nơi cấp CCCD/CMND")
                .HasColumnName("citizen_issue_place");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasColumnType("citext")
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasComment("Họ và tên")
                .HasColumnName("fullname");
            entity.Property(e => e.Gender)
                .HasComment("Giới tính: male/female/other/unspecified")
                .HasColumnName("gender");
            entity.Property(e => e.HeightCm)
                .HasPrecision(5, 2)
                .HasComment("Chiều cao (cm), 0–300")
                .HasColumnName("height_cm");
            entity.Property(e => e.IsPublic)
                .HasDefaultValue(false)
                .HasColumnName("is_public");
            entity.Property(e => e.KycStatus)
                .HasColumnType("kyc_status_enum")
                .HasColumnName("kyc_status");
            entity.Property(e => e.KycNote).HasColumnName("kyc_note");
            entity.Property(e => e.OperatingLocation)
                .HasComment("Khu vực/Nơi hoạt động chính")
                .HasColumnName("operating_location");
            entity.Property(e => e.RatingAvg)
                .HasPrecision(3, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("rating_avg");
            entity.Property(e => e.RatingCount)
                .HasDefaultValue(0)
                .HasColumnName("rating_count");
            entity.Property(e => e.ServiceRadiusKm)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("10.0")
                .HasColumnName("service_radius_km");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(20)
                .HasComment("Mã số thuế (10 hoặc 13 chữ số)")
                .HasColumnName("tax_code");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.WeightKg)
                .HasPrecision(5, 2)
                .HasComment("Cân nặng (kg), 0–500")
                .HasColumnName("weight_kg");
            entity.Property(e => e.YearsExperience).HasColumnName("years_experience");

            entity.HasMany(d => d.Sports).WithMany(p => p.Coaches)
                .UsingEntity<Dictionary<string, object>>(
                    "CoachSport",
                    r => r.HasOne<Sport>().WithMany()
                        .HasForeignKey("SportId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("coach_sport_sport_id_fkey"),
                    l => l.HasOne<CoachProfile>().WithMany()
                        .HasForeignKey("CoachId")
                        .HasConstraintName("fk_coach_sport_profile"),
                    j =>
                    {
                        j.HasKey("CoachId", "SportId").HasName("coach_sport_pkey");
                        j.ToTable("coach_sport");
                        j.IndexerProperty<Guid>("CoachId").HasColumnName("coach_id");
                        j.IndexerProperty<Guid>("SportId").HasColumnName("sport_id");
                    });
        });

        modelBuilder.Entity<CoachService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("coach_service_pkey");

            entity.ToTable("coach_service");

            entity.HasIndex(e => e.CoachId, "idx_coach_service_coach");

            entity.HasIndex(e => e.SportId, "idx_coach_service_sport");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LocationNote).HasColumnName("location_note");
            entity.Property(e => e.OnlineAvailable)
                .HasDefaultValue(true)
                .HasColumnName("online_available");
            entity.Property(e => e.OnsiteAvailable)
                .HasDefaultValue(true)
                .HasColumnName("onsite_available");
            entity.Property(e => e.PriceVnd).HasColumnName("price_vnd");
            entity.Property(e => e.SessionsTotal).HasColumnName("sessions_total");
            entity.Property(e => e.SportId).HasColumnName("sport_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Coach).WithMany(p => p.CoachServices)
                .HasForeignKey(d => d.CoachId)
                .HasConstraintName("fk_coach_service_profile");

            entity.HasOne(d => d.Sport).WithMany(p => p.CoachServices)
                .HasForeignKey(d => d.SportId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("coach_service_sport_id_fkey");
        });

        modelBuilder.Entity<KycRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("kyc_record_pkey");

            entity.ToTable("kyc_record");

            entity.HasIndex(e => e.CoachId, "idx_kyc_coach");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AdminNote).HasColumnName("admin_note");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.IdDocumentUrl).HasColumnName("id_document_url");
            entity.Property(e => e.Status)
                .HasColumnType("kyc_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("submitted_at");

            entity.HasOne(d => d.Coach).WithMany(p => p.KycRecords)
                .HasForeignKey(d => d.CoachId)
                .HasConstraintName("fk_kyc_record_profile");
        });

        modelBuilder.Entity<Sport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sport_pkey");

            entity.ToTable("sport");

            entity.HasIndex(e => e.Id, "ux_sport_id_new").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}