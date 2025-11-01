using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LunkvayAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AvatarsImgDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "file_name",
                table: "avatars",
                newName: "img_db_url");

            migrationBuilder.AddColumn<string>(
                name: "img_db_delete_url",
                table: "avatars",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "img_db_id",
                table: "avatars",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "img_db_delete_url",
                table: "avatars");

            migrationBuilder.DropColumn(
                name: "img_db_id",
                table: "avatars");

            migrationBuilder.RenameColumn(
                name: "img_db_url",
                table: "avatars",
                newName: "file_name");
        }
    }
}
