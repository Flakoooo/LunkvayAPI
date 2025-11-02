using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LunkvayAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChatChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "chat_images",
                newName: "img_db_url");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "chat_members",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "chat_members",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "chat_members",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "chat_members",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "img_db_delete_url",
                table: "chat_images",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "img_db_id",
                table: "chat_images",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "chat_members");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "chat_members");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "chat_members");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "chat_members");

            migrationBuilder.DropColumn(
                name: "img_db_delete_url",
                table: "chat_images");

            migrationBuilder.DropColumn(
                name: "img_db_id",
                table: "chat_images");

            migrationBuilder.RenameColumn(
                name: "img_db_url",
                table: "chat_images",
                newName: "file_name");
        }
    }
}
