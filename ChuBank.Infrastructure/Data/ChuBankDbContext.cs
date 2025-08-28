using Microsoft.EntityFrameworkCore;
using ChuBank.Domain.Entities;

namespace ChuBank.Infrastructure.Data;

public class ChuBankDbContext : DbContext
{
    public ChuBankDbContext(DbContextOptions<ChuBankDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Transfer> Transfers { get; set; } = null!;
    public DbSet<Statement> Statements { get; set; } = null!;
    public DbSet<StatementEntry> StatementEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.HolderName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.AccountNumber).IsUnique();
        });

        // Transfer configuration
        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.FromAccount)
                .WithMany(a => a.SentTransfers)
                .HasForeignKey(e => e.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ToAccount)
                .WithMany(a => a.ReceivedTransfers)
                .HasForeignKey(e => e.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Statement configuration
        modelBuilder.Entity<Statement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OpeningBalance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ClosingBalance).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StatementEntry configuration
        modelBuilder.Entity<StatementEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).IsRequired().HasMaxLength(10);

            entity.HasOne(e => e.Statement)
                .WithMany(s => s.Entries)
                .HasForeignKey(e => e.StatementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Transfer)
                .WithMany()
                .HasForeignKey(e => e.TransferId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        base.OnModelCreating(modelBuilder);
    }
}
