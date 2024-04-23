using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Add_ContentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ComicChapterId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CompletionRate = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentProgresses", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentProgresses");
        }
    }
}
