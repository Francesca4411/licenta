using Microsoft.EntityFrameworkCore;
using StudyManagement.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using StudyManagement.Services;

namespace StudyManagement.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentUserService _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUser) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Subject> Subjects => Set<Subject>();
        public DbSet<Evaluation> Evaluations => Set<Evaluation>();
        public DbSet<StudySession> StudySessions => Set<StudySession>();
        public DbSet<PomodoroSession> PomodoroSessions => Set<PomodoroSession>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasMany(s => s.Evaluations)
                    .WithOne(e => e.Subject)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.IdentityUserId);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(s => s.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(s => s.IdentityUserId == _currentUser.UserId);
            });

            modelBuilder.Entity<StudySession>(entity =>
            {
                entity.HasOne(s => s.Subject)
                    .WithMany()
                    .HasForeignKey(s => s.SubjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.IdentityUserId);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(s => s.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(s => s.IdentityUserId == _currentUser.UserId);
            });

            modelBuilder.Entity<PomodoroSession>(entity =>
            {
                entity.HasIndex(p => p.IdentityUserId);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(p => p.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(p => p.IdentityUserId == _currentUser.UserId);
            });

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasIndex(p => p.IdentityUserId).IsUnique();
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(p => p.IdentityUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUser.UserId;
            if (!string.IsNullOrEmpty(userId))
            {
                foreach (var entry in ChangeTracker.Entries<Subject>()
                    .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.IdentityUserId)))
                {
                    entry.Entity.IdentityUserId = userId;
                }

                foreach (var entry in ChangeTracker.Entries<StudySession>()
                    .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.IdentityUserId)))
                {
                    entry.Entity.IdentityUserId = userId;
                }

                foreach (var entry in ChangeTracker.Entries<PomodoroSession>()
                    .Where(e => e.State == EntityState.Added && string.IsNullOrEmpty(e.Entity.IdentityUserId)))
                {
                    entry.Entity.IdentityUserId = userId;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
