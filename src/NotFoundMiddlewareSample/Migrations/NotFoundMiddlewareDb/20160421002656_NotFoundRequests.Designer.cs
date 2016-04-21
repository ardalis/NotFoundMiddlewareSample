using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using NotFoundMiddlewareSample.Middleware;

namespace NotFoundMiddlewareSample.Migrations.NotFoundMiddlewareDb
{
    [DbContext(typeof(NotFoundMiddlewareDbContext))]
    [Migration("20160421002656_NotFoundRequests")]
    partial class NotFoundRequests
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
