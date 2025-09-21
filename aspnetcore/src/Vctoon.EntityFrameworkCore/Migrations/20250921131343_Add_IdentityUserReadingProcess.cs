using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Add_IdentityUserReadingProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppIdentityUserReadingProcesses",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    LastReadTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ComicId = table.Column<Guid>(type: "uuid", nullable: true),
                    VideoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppIdentityUserReadingProcesses", x => new { x.UserId, x.MediumId });
                    table.ForeignKey(
                        name: "FK_AppIdentityUserReadingProcesses_AppComics_ComicId",
                        column: x => x.ComicId,
                        principalTable: "AppComics",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppIdentityUserReadingProcesses_AppVideos_VideoId",
                        column: x => x.VideoId,
                        principalTable: "AppVideos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_ComicId",
                table: "AppIdentityUserReadingProcesses",
                column: "ComicId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_VideoId",
                table: "AppIdentityUserReadingProcesses",
                column: "VideoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppIdentityUserReadingProcesses");
        }
    }
}
