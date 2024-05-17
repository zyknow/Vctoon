using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vctoon.Migrations
{
    /// <inheritdoc />
    public partial class Change_Entities_To_FullAudited : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "_Tags");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "_TagGroups");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "_Libraries");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "_Comics");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "_Tags",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "_TagGroups",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "_Libraries",
                newName: "CreationTime");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "_Comics",
                newName: "CreationTime");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "_Tags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "_Tags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "_Tags",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "_TagGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "_TagGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "_TagGroups",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "_Libraries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "_Libraries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "_Libraries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "_Comics",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "_Comics",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "_Comics",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "_ComicChapters",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "_ComicChapters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "_ComicChapters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "_ComicChapters",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "_Tags");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "_Tags");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "_Tags");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "_TagGroups");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "_TagGroups");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "_TagGroups");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "_Libraries");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "_Libraries");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "_Libraries");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "_Comics");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "_Comics");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "_Comics");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "_ComicChapters");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "_ComicChapters");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "_ComicChapters");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "_ComicChapters");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "_Tags",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "_TagGroups",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "_Libraries",
                newName: "ExtraProperties");

            migrationBuilder.RenameColumn(
                name: "CreationTime",
                table: "_Comics",
                newName: "ExtraProperties");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "_Tags",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "_TagGroups",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "_Libraries",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "_Comics",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }
    }
}
