using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Change_Medium_Process_Id_Binding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppComics_MediumId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppVideos_MediumId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppIdentityUserReadingProcesses",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropIndex(
                name: "IX_AppIdentityUserReadingProcesses_MediumId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.RenameColumn(
                name: "MediumId",
                table: "AppIdentityUserReadingProcesses",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "ComicId",
                table: "AppIdentityUserReadingProcesses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "AppIdentityUserReadingProcesses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppIdentityUserReadingProcesses",
                table: "AppIdentityUserReadingProcesses",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_ComicId",
                table: "AppIdentityUserReadingProcesses",
                column: "ComicId");

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_VideoId",
                table: "AppIdentityUserReadingProcesses",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppComics_ComicId",
                table: "AppIdentityUserReadingProcesses",
                column: "ComicId",
                principalTable: "AppComics",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppVideos_VideoId",
                table: "AppIdentityUserReadingProcesses",
                column: "VideoId",
                principalTable: "AppVideos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppComics_ComicId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppVideos_VideoId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppIdentityUserReadingProcesses",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropIndex(
                name: "IX_AppIdentityUserReadingProcesses_ComicId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropIndex(
                name: "IX_AppIdentityUserReadingProcesses_VideoId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropColumn(
                name: "ComicId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "AppIdentityUserReadingProcesses");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AppIdentityUserReadingProcesses",
                newName: "MediumId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppIdentityUserReadingProcesses",
                table: "AppIdentityUserReadingProcesses",
                columns: new[] { "UserId", "MediumId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_MediumId",
                table: "AppIdentityUserReadingProcesses",
                column: "MediumId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppComics_MediumId",
                table: "AppIdentityUserReadingProcesses",
                column: "MediumId",
                principalTable: "AppComics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppIdentityUserReadingProcesses_AppVideos_MediumId",
                table: "AppIdentityUserReadingProcesses",
                column: "MediumId",
                principalTable: "AppVideos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
