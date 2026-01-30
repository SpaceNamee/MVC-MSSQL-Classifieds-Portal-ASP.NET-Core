using Microsoft.EntityFrameworkCore;

namespace MVC___MSSQL_Classifieds_Portal.Models
{
    public class ClassifieldsContext: DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public ClassifieldsContext(DbContextOptions<ClassifieldsContext> option)
            : base(option)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------- Indexes -----------
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Listing indexes for filtering and sorting
            modelBuilder.Entity<Listing>()
                .HasIndex(l => new { l.CategoryId, l.IsActive });

            modelBuilder.Entity<Listing>()
                .HasIndex(l => new { l.UserId, l.IsActive });

            modelBuilder.Entity<Listing>()
                .HasIndex(l => l.Price);

            modelBuilder.Entity<Listing>()
                .HasIndex(l => l.CreatedAt);

            modelBuilder.Entity<Listing>()
                .HasIndex(l => l.Title);

            // AuditLog indexes
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => a.Timestamp)
                .IsDescending();

            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.EntityName, a.EntityId });


            // This tells EF Core: Automatically filter out listings where IsActive == false in ALL queries. 
            //So when you write this: var listings = _context.Listings.ToList(); EF Core internally adds:WHERE IsActive = 1 without you having to write it.
            //This is how soft delete works. Instead of deleting a row from the database, you mark it as inactive:
            // var all = _context.Listings.IgnoreQueryFilters().ToList() - This returns all listings, active and inactive.

            // Query filter for soft delete
            modelBuilder.Entity<Listing>()
                .HasQueryFilter(l => l.IsActive);

            // Relationships
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.User)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Category)
                .WithMany(c => c.Listings)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(c => c.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
