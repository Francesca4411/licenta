using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class StudySessionCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActualDurationMinutes",
                table: "StudySessions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DifficultyLabel",
                table: "StudySessions",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "StudySessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "StudySessions",
                type: "TEXT",
                maxLength: 600,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualDurationMinutes",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "DifficultyLabel",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "StudySessions");
        }
    }
}
