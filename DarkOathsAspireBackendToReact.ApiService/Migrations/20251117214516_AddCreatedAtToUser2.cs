using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarkOathsAspireBackendToReact.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12345678-1234-1234-1234-123456789012"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 21, 45, 15, 922, DateTimeKind.Utc).AddTicks(477));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12345678-1234-1234-1234-123456789012"),
                column: "CreatedAt",
                value: new DateTime(2025, 11, 17, 21, 41, 52, 189, DateTimeKind.Utc).AddTicks(3595));
        }
    }
}
