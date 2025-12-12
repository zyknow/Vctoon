using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class AddSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSeries",
                table: "AppMediums",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AppMediumSeriesLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediumId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppMediumSeriesLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppMediumSeriesLinks_AppMediums_MediumId",
                        column: x => x.MediumId,
                        principalTable: "AppMediums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppMediumSeriesLinks_AppMediums_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "AppMediums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppMediumSeriesLinks_MediumId",
                table: "AppMediumSeriesLinks",
                column: "MediumId");

            migrationBuilder.CreateIndex(
                name: "IX_AppMediumSeriesLinks_SeriesId_MediumId",
                table: "AppMediumSeriesLinks",
                columns: new[] { "SeriesId", "MediumId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppMediumSeriesLinks_SeriesId_Sort",
                table: "AppMediumSeriesLinks",
                columns: new[] { "SeriesId", "Sort" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppMediumSeriesLinks");

            migrationBuilder.DropColumn(
                name: "IsSeries",
                table: "AppMediums");
        }
    }
}
