using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace NotFoundMiddlewareSample.Migrations.NotFoundMiddlewareDb
{
    public partial class NotFoundRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotFoundRequest",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    CorrectedPath = table.Column<string>(nullable: true),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotFoundRequest", x => x.Path);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("NotFoundRequest");
        }
    }
}
