using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class MediumEntity_Add_LibraryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppVideos_LibraryId",
                table: "AppVideos",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppComics_LibraryId",
                table: "AppComics",
                column: "LibraryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppComics_AppLibraries_LibraryId",
                table: "AppComics",
                column: "LibraryId",
                principalTable: "AppLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppVideos_AppLibraries_LibraryId",
                table: "AppVideos",
                column: "LibraryId",
                principalTable: "AppLibraries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppComics_AppLibraries_LibraryId",
                table: "AppComics");

            migrationBuilder.DropForeignKey(
                name: "FK_AppVideos_AppLibraries_LibraryId",
                table: "AppVideos");

            migrationBuilder.DropIndex(
                name: "IX_AppVideos_LibraryId",
                table: "AppVideos");

            migrationBuilder.DropIndex(
                name: "IX_AppComics_LibraryId",
                table: "AppComics");
        }
    }
}
