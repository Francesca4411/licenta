using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileWeeklyGoalHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WeeklyGoalHours",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 12);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyGoalHours",
                table: "UserProfiles");
        }
    }
}
