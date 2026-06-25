using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class StudySessionsActualStartAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartAt",
                table: "StudySessions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualStartAt",
                table: "StudySessions");
        }
    }
}
