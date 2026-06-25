using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class ScopeStudyDataToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Subjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "StudySessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "PomodoroSessions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_IdentityUserId",
                table: "Subjects",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySessions_IdentityUserId",
                table: "StudySessions",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_IdentityUserId",
                table: "PomodoroSessions",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PomodoroSessions_AspNetUsers_IdentityUserId",
                table: "PomodoroSessions",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySessions_AspNetUsers_IdentityUserId",
                table: "StudySessions",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_AspNetUsers_IdentityUserId",
                table: "Subjects",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PomodoroSessions_AspNetUsers_IdentityUserId",
                table: "PomodoroSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySessions_AspNetUsers_IdentityUserId",
                table: "StudySessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_AspNetUsers_IdentityUserId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_IdentityUserId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_StudySessions_IdentityUserId",
                table: "StudySessions");

            migrationBuilder.DropIndex(
                name: "IX_PomodoroSessions_IdentityUserId",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "StudySessions");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "PomodoroSessions");
        }
    }
}
