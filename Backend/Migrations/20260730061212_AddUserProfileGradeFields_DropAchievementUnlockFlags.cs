using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileGradeFields_DropAchievementUnlockFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUnlocked",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "ProgressPercent",
                table: "Achievements");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Users",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KhoiHocId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_KhoiHocId",
                table: "Users",
                column: "KhoiHocId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_KhoiHocId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KhoiHocId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsUnlocked",
                table: "Achievements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProgressPercent",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
