using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LunkvayAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class FriendshipsLabelsChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "friendship_labels",
                keyColumn: "label",
                keyValue: null,
                column: "label",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "friendship_labels",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "label",
                table: "friendship_labels",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
