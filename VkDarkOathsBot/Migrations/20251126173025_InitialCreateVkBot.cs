using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VkDarkOathsBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateVkBot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VkUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VkId = table.Column<string>(type: "text", nullable: false),
                    LinkedAuthUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkingCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VkUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VkUsers_VkId",
                table: "VkUsers",
                column: "VkId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VkUsers");
        }
    }
}
