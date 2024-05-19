using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Comic_ComicChapter_Nouse_Field : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "_ImageFiles");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "_ImageFiles");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "_ComicChapters");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "_ComicChapters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Height",
                table: "_ImageFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Width",
                table: "_ImageFiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "PageCount",
                table: "_ComicChapters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "_ComicChapters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
