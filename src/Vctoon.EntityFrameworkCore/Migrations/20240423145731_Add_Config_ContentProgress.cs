using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Add_Config_ContentProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContentProgresses",
                table: "ContentProgresses");

            migrationBuilder.RenameTable(
                name: "ContentProgresses",
                newName: "_ContentProgresses");

            migrationBuilder.AddColumn<Guid>(
                name: "ComicId",
                table: "_ContentProgresses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__ContentProgresses",
                table: "_ContentProgresses",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX__ContentProgresses_ComicChapterId",
                table: "_ContentProgresses",
                column: "ComicChapterId");

            migrationBuilder.CreateIndex(
                name: "IX__ContentProgresses_ComicId",
                table: "_ContentProgresses",
                column: "ComicId");

            migrationBuilder.CreateIndex(
                name: "IX__ContentProgresses_UserId",
                table: "_ContentProgresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK__ContentProgresses_AbpUsers_UserId",
                table: "_ContentProgresses",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__ContentProgresses__ComicChapters_ComicChapterId",
                table: "_ContentProgresses",
                column: "ComicChapterId",
                principalTable: "_ComicChapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__ContentProgresses__Comics_ComicId",
                table: "_ContentProgresses",
                column: "ComicId",
                principalTable: "_Comics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK__ContentProgresses_AbpUsers_UserId",
                table: "_ContentProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK__ContentProgresses__ComicChapters_ComicChapterId",
                table: "_ContentProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK__ContentProgresses__Comics_ComicId",
                table: "_ContentProgresses");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ContentProgresses",
                table: "_ContentProgresses");

            migrationBuilder.DropIndex(
                name: "IX__ContentProgresses_ComicChapterId",
                table: "_ContentProgresses");

            migrationBuilder.DropIndex(
                name: "IX__ContentProgresses_ComicId",
                table: "_ContentProgresses");

            migrationBuilder.DropIndex(
                name: "IX__ContentProgresses_UserId",
                table: "_ContentProgresses");

            migrationBuilder.DropColumn(
                name: "ComicId",
                table: "_ContentProgresses");

            migrationBuilder.RenameTable(
                name: "_ContentProgresses",
                newName: "ContentProgresses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContentProgresses",
                table: "ContentProgresses",
                column: "Id");
        }
    }
}
