using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserProfileUpdatedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "UserProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);
        }
    }
}
