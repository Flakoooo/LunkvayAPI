using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LunkvayAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "chats",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "chats");
        }
    }
}
