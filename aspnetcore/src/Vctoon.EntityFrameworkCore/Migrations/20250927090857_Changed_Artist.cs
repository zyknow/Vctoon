using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Changed_Artist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "AppArtists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AppArtists",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "AppArtists",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppArtists",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AppArtists",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "AppArtists",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Slug",
                table: "AppArtists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
