using Microsoft.Data.Entity;

namespace NotFoundMiddlewareSample.Middleware
{
    public class NotFoundMiddlewareDbContext : DbContext
    {
        public DbSet<NotFoundRequest> NotFoundRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<NotFoundRequest>().HasKey(r => r.Path);
        }
    }
}