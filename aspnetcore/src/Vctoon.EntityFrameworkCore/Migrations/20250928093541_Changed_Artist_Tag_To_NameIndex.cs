using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Changed_Artist_Tag_To_NameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppTags_Name",
                table: "AppTags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppArtists_Name",
                table: "AppArtists",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppTags_Name",
                table: "AppTags");

            migrationBuilder.DropIndex(
                name: "IX_AppArtists_Name",
                table: "AppArtists");
        }
    }
}
