using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class MediumEntity_Add_LibraryPathId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryPathId",
                table: "AppVideos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LibraryPathId",
                table: "AppComics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppVideos_LibraryPathId",
                table: "AppVideos",
                column: "LibraryPathId");

            migrationBuilder.CreateIndex(
                name: "IX_AppComics_LibraryPathId",
                table: "AppComics",
                column: "LibraryPathId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppComics_AppLibraryPaths_LibraryPathId",
                table: "AppComics",
                column: "LibraryPathId",
                principalTable: "AppLibraryPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppVideos_AppLibraryPaths_LibraryPathId",
                table: "AppVideos",
                column: "LibraryPathId",
                principalTable: "AppLibraryPaths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppComics_AppLibraryPaths_LibraryPathId",
                table: "AppComics");

            migrationBuilder.DropForeignKey(
                name: "FK_AppVideos_AppLibraryPaths_LibraryPathId",
                table: "AppVideos");

            migrationBuilder.DropIndex(
                name: "IX_AppVideos_LibraryPathId",
                table: "AppVideos");

            migrationBuilder.DropIndex(
                name: "IX_AppComics_LibraryPathId",
                table: "AppComics");

            migrationBuilder.DropColumn(
                name: "LibraryPathId",
                table: "AppVideos");

            migrationBuilder.DropColumn(
                name: "LibraryPathId",
                table: "AppComics");

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
    }
}
