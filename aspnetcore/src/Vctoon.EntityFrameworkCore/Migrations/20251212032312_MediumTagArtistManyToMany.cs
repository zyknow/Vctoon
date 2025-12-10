using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class MediumTagArtistManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppMediumArtists",
                columns: table => new
                {
                    MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtistId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediumArtists", x => new { x.MediumId, x.ArtistId });
                    table.ForeignKey(
                        name: "FK_AppMediumArtists_AppArtists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "AppArtists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppMediumArtists_AppMediums_MediumId",
                        column: x => x.MediumId,
                        principalTable: "AppMediums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppMediumTags",
                columns: table => new
                {
                    MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediumTags", x => new { x.MediumId, x.TagId });
                    table.ForeignKey(
                        name: "FK_AppMediumTags_AppMediums_MediumId",
                        column: x => x.MediumId,
                        principalTable: "AppMediums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppMediumTags_AppTags_TagId",
                        column: x => x.TagId,
                        principalTable: "AppTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Preserve existing one-to-many links by copying MediumId into the new join tables
            migrationBuilder.Sql(
                "INSERT INTO \"AppMediumArtists\" (\"MediumId\", \"ArtistId\") " +
                "SELECT \"MediumId\", \"Id\" FROM \"AppArtists\" WHERE \"MediumId\" IS NOT NULL;");

            migrationBuilder.Sql(
                "INSERT INTO \"AppMediumTags\" (\"MediumId\", \"TagId\") " +
                "SELECT \"MediumId\", \"Id\" FROM \"AppTags\" WHERE \"MediumId\" IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_AppArtists_AppMediums_MediumId",
                table: "AppArtists");

            migrationBuilder.DropForeignKey(
                name: "FK_AppTags_AppMediums_MediumId",
                table: "AppTags");

            migrationBuilder.DropIndex(
                name: "IX_AppTags_MediumId",
                table: "AppTags");

            migrationBuilder.DropIndex(
                name: "IX_AppArtists_MediumId",
                table: "AppArtists");

            migrationBuilder.DropColumn(
                name: "MediumId",
                table: "AppTags");

            migrationBuilder.DropColumn(
                name: "MediumId",
                table: "AppArtists");

            migrationBuilder.CreateIndex(
                name: "IX_AppMediumArtists_ArtistId",
                table: "AppMediumArtists",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMediumTags_TagId",
                table: "AppMediumTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMediumArtists");

            migrationBuilder.DropTable(
                name: "AppMediumTags");

            migrationBuilder.AddColumn<Guid>(
                name: "MediumId",
                table: "AppTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MediumId",
                table: "AppArtists",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppTags_MediumId",
                table: "AppTags",
                column: "MediumId");

            migrationBuilder.CreateIndex(
                name: "IX_AppArtists_MediumId",
                table: "AppArtists",
                column: "MediumId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppArtists_AppMediums_MediumId",
                table: "AppArtists",
                column: "MediumId",
                principalTable: "AppMediums",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppTags_AppMediums_MediumId",
                table: "AppTags",
                column: "MediumId",
                principalTable: "AppMediums",
                principalColumn: "Id");
        }
    }
}
