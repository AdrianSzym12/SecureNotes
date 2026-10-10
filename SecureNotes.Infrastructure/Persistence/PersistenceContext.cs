
using Microsoft.EntityFrameworkCore;
using SecureNotes.Domain.Entities;
using SecureNotes.Domain.ValueObjects;

namespace SecureNotes.Infrastructure.Persistence
{
    public class PersistenceContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Note> Notes { get; set; } = null!;

        public PersistenceContext(
            DbContextOptions<PersistenceContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USERS

            modelBuilder
                .Entity<User>()
                .ToTable(nameof(Users))
                .HasKey(u => u.Id);

            modelBuilder
                .Entity<User>()
                .Property(u => u.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<User>()
                .Property(u => u.Email)
                .HasConversion(
                    email => email.Value,
                    value => new Email(value))
                .HasMaxLength(Email.MaxLength)
                .IsRequired();

            modelBuilder
                .Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder
                .Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            modelBuilder
                .Entity<User>()
                .Property(u => u.CreatedAtUtc)
                .HasColumnType("datetime(6)")
                .IsRequired();

            // NOTES

            modelBuilder
                .Entity<Note>()
                .ToTable(nameof(Notes))
                .HasKey(n => n.Id);

            modelBuilder
                .Entity<Note>()
                .Property(n => n.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<Note>()
                .Property(n => n.UserId)
                .IsRequired();

            modelBuilder
                .Entity<Note>()
                .HasIndex(n => n.UserId);

            modelBuilder
                .Entity<Note>()
                .Property(n => n.Title)
                .HasMaxLength(Note.MaxTitleLength)
                .IsRequired();

            modelBuilder
                .Entity<Note>()
                .Property(n => n.Content)
                .HasColumnType("text")
                .IsRequired();

            modelBuilder
                .Entity<Note>()
                .Property(n => n.CreatedAtUtc)
                .HasColumnType("datetime(6)")
                .IsRequired();

            modelBuilder
                .Entity<Note>()
                .Property(n => n.UpdatedAtUtc)
                .HasColumnType("datetime(6)")
                .IsRequired(false);

            // USER -> NOTES

            modelBuilder
                .Entity<User>()
                .HasMany<Note>()
                .WithOne()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
