using Microsoft.EntityFrameworkCore;
using Sigebi.Domain.Entities;

namespace Sigebi.Infrastructure.Persistence;

public sealed class SigebiDbContext(DbContextOptions<SigebiDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookCopy> BookCopies => Set<BookCopy>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanRequest> LoanRequests => Set<LoanRequest>();
    public DbSet<Penalty> Penalties => Set<Penalty>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(200);
        });

        modelBuilder.Entity<Book>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Author).HasMaxLength(200);
            e.Property(x => x.Isbn).HasMaxLength(32);
            e.Property(x => x.Category).HasMaxLength(120);
            e.HasMany(x => x.Copies)
                .WithOne(x => x.Book)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookCopy>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InventoryCode).HasMaxLength(64);
            e.HasIndex(x => x.InventoryCode).IsUnique();
        });

        modelBuilder.Entity<Loan>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.UserId);
            e.HasOne(x => x.BookCopy)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoanRequest>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
                .WithMany(x => x.LoanRequests)
                .HasForeignKey(x => x.UserId);
            e.HasOne(x => x.BookCopy)
                .WithMany(x => x.LoanRequests)
                .HasForeignKey(x => x.BookCopyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Penalty>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(500);
            e.HasOne(x => x.User)
                .WithMany(x => x.Penalties)
                .HasForeignKey(x => x.UserId);
            e.HasOne(x => x.Loan)
                .WithMany(x => x.Penalties)
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EntityType).HasMaxLength(120);
            e.Property(x => x.EntityId).HasMaxLength(64);
            e.Property(x => x.Details).HasMaxLength(2000);
        });
    }
}
