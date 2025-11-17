using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkOathsAspireBackendToReact.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUserWithStaticDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12345678-1234-1234-1234-123456789012"),
                column: "CreatedAt",
                value: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12345678-1234-1234-1234-123456789012"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 21, 45, 15, 922, DateTimeKind.Utc).AddTicks(477));
        }
    }
}
