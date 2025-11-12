using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CV> CVs { get; set; }
        public DbSet<PersonalInfo> PersonalInfos { get; set; }
        public DbSet<TimelineItem> TimelineItems { get; set; }
        public DbSet<Skills> Skills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure CV entity
            modelBuilder.Entity<CV>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.RawContent).HasMaxLength(10000);
                entity.Property(e => e.ParsedContent).HasMaxLength(10000);

                // One-to-one relationship with PersonalInfo
                entity.HasOne(e => e.PersonalInfo)
                      .WithOne()
                      .HasForeignKey<PersonalInfo>("CVId")
                      .OnDelete(DeleteBehavior.Cascade);

                // One-to-many relationships
                entity.HasMany(e => e.TimelineItems)
                      .WithOne()
                      .HasForeignKey("CVId")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Skills)
                      .WithOne()
                      .HasForeignKey("CVId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure PersonalInfo entity
            modelBuilder.Entity<PersonalInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.LinkedIn).HasMaxLength(255);
                entity.Property(e => e.Summary).HasMaxLength(500);
            });

            // Configure TimelineItem entity
            modelBuilder.Entity<TimelineItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Organization).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subtitle).HasMaxLength(100);
                entity.Property(e => e.Duration).HasMaxLength(20);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Tech).HasMaxLength(100);
                entity.Property(e => e.Grade).HasMaxLength(10);

                // Configure BulletPoints as JSON column
                entity.Property(e => e.BulletPoints)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                      );
            });

            // Configure Skills entity
            modelBuilder.Entity<Skills>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);

                // Configure SkillsList as JSON column
                entity.Property(e => e.SkillsList)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                      );
            });

            // Configure many-to-many relationships for Skills
            modelBuilder.Entity<Skills>()
                .HasMany(s => s.RelatedTimelineItems)
                .WithMany()
                .UsingEntity(j => j.ToTable("SkillsTimelineItems"));
        }

        public override int SaveChanges()
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ConvertDateTimesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue != null)
                    {
                        var dateTime = (DateTime)property.CurrentValue;
                        if (dateTime.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                        }
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue != null)
                    {
                        var dateTime = (DateTime?)property.CurrentValue;
                        if (dateTime.HasValue && dateTime.Value.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }
}
