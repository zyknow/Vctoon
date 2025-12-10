using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class IdentityUserReadingProcessUserMediumUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "AppIdentityUserReadingProcesses" p
                USING (
                    SELECT "Id",
                           ROW_NUMBER() OVER (
                               PARTITION BY "UserId", "MediumId"
                               ORDER BY "LastReadTime" DESC NULLS LAST, "Progress" DESC, "Id" DESC
                           ) AS rn
                    FROM "AppIdentityUserReadingProcesses"
                ) d
                WHERE p."Id" = d."Id" AND d.rn > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_AppIdentityUserReadingProcesses_UserId_MediumId",
                table: "AppIdentityUserReadingProcesses",
                columns: new[] { "UserId", "MediumId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppIdentityUserReadingProcesses_UserId_MediumId",
                table: "AppIdentityUserReadingProcesses");
        }
    }
}
