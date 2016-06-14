using Microsoft.EntityFrameworkCore;

namespace NotFoundMiddlewareSample.Middleware
{
    public class NotFoundMiddlewareDbContext : DbContext
    {
        public NotFoundMiddlewareDbContext(DbContextOptions<NotFoundMiddlewareDbContext> options)
            : base(options)
        {
        }

        public DbSet<NotFoundRequest> NotFoundRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NotFoundRequest>()
                .ToTable("NotFoundRequest")
                .HasKey(r => r.Path);
        }
    }
}