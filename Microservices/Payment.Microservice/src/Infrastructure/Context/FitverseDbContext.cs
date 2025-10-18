using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public partial class FitverseDbContext : DbContext
{
    public FitverseDbContext()
    {
    }

    public FitverseDbContext(DbContextOptions<FitverseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletBalance> WalletBalances { get; set; }

    public virtual DbSet<WalletJournal> WalletJournals { get; set; }

    public virtual DbSet<WalletLedgerEntry> WalletLedgerEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<Dc>("public", "dc_enum")
            .HasPostgresEnum<Gateway>("public", "gateway_enum")
            .HasPostgresEnum<PaymentStatus>("public", "payment_status_enum")
            .HasPostgresEnum<WalletAccountType>("public", "wallet_account_type_enum")
            .HasPostgresEnum<WalletJournalStatus>("public", "wallet_journal_status_enum")
            .HasPostgresEnum<WalletJournalType>("public", "wallet_journal_type_enum")
            .HasPostgresEnum<WalletStatus>("public", "wallet_status_enum")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("citext");

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AmountVnd).HasColumnName("amount_vnd");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.GatewayMeta)
                .HasColumnType("jsonb")
                .HasColumnName("gateway_meta");
            entity.Property(e => e.GatewayTxnId).HasColumnName("gateway_txn_id");
            entity.Property(e => e.Gateway)
                .HasColumnType("gateway_enum")
                .HasColumnName("gateway");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.RefundAmountVnd)
                .HasDefaultValue(0L)
                .HasColumnName("refund_amount_vnd");
            entity.Property(e => e.Status)
                .HasColumnType("payment_status_enum")
                .HasColumnName("status");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_pkey");

            entity.ToTable("wallet");

            entity.HasIndex(e => e.UserId, "wallet_user_id_key").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsSystem)
                .HasDefaultValue(false)
                .HasColumnName("is_system");
            entity.Property(e => e.Status)
                .HasColumnType("wallet_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<WalletBalance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_balance_pkey");

            entity.ToTable("wallet_balance");

            entity.HasIndex(e => e.WalletId, "idx_wallet_balance_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BalanceVnd)
                .HasDefaultValue(0L)
                .HasColumnName("balance_vnd");
            entity.Property(e => e.AccountType)
                .HasColumnType("wallet_account_type_enum")
                .HasColumnName("account_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletBalances)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("wallet_balance_wallet_id_fkey");
        });

        modelBuilder.Entity<WalletJournal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_journal_pkey");

            entity.ToTable("wallet_journal");

            entity.HasIndex(e => e.BookingId, "idx_journal_booking");

            entity.HasIndex(e => e.PaymentId, "idx_journal_payment");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Status)
                .HasColumnType("wallet_journal_status_enum")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasColumnType("wallet_journal_type_enum")
                .HasColumnName("type");
            entity.Property(e => e.PostedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("posted_at");
        });

        modelBuilder.Entity<WalletLedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_ledger_entry_pkey");

            entity.ToTable("wallet_ledger_entry");

            entity.HasIndex(e => e.JournalId, "idx_ledger_journal");

            entity.HasIndex(e => e.WalletId, "idx_ledger_wallet");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.AmountVnd).HasColumnName("amount_vnd");
            entity.Property(e => e.Dc)
                .HasColumnType("dc_enum")
                .HasColumnName("dc");
            entity.Property(e => e.AccountType)
                .HasColumnType("wallet_account_type_enum")
                .HasColumnName("account_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.JournalId).HasColumnName("journal_id");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Journal).WithMany(p => p.WalletLedgerEntries)
                .HasForeignKey(d => d.JournalId)
                .HasConstraintName("wallet_ledger_entry_journal_id_fkey");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletLedgerEntries)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("wallet_ledger_entry_wallet_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
