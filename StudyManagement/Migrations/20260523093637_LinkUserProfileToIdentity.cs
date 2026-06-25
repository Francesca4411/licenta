using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyManagement.Migrations
{
    /// <inheritdoc />
    public partial class LinkUserProfileToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles",
                column: "IdentityUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_AspNetUsers_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_IdentityUserId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "UserProfiles");
        }
    }
}
