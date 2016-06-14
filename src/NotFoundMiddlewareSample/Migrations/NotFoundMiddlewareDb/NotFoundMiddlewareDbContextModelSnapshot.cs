using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NotFoundMiddlewareSample.Middleware;

namespace NotFoundMiddlewareSample.Migrations.NotFoundMiddlewareDb
{
    [DbContext(typeof(NotFoundMiddlewareDbContext))]
    partial class NotFoundMiddlewareDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("NotFoundMiddlewareSample.Middleware.NotFoundRequest", b =>
                {
                    b.Property<string>("Path");

                    b.Property<string>("CorrectedPath");

                    b.Property<int>("Count");

                    b.HasKey("Path");
                });
        }
    }
}
