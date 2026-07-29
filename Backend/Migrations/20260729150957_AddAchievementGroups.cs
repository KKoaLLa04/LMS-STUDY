using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementGroups_Code",
                table: "AchievementGroups",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            // Seed 3 nhóm mặc định tương ứng với enum AchievementCategory cũ (Code giữ nguyên
            // giá trị enum để backfill GroupId bên dưới và để AchievementDto.Category không đổi
            // giá trị cho các huy hiệu đã có — Frontend/src/app/clients vẫn đọc đúng).
            migrationBuilder.Sql(@"
                INSERT INTO [AchievementGroups] ([Name], [Code], [OrderNumber], [IsDeleted])
                VALUES (N'Học tập', 'HocTap', 0, 0), (N'Chuyên cần', 'ChuyenCan', 1, 0), (N'Tương tác', 'TuongTac', 2, 0);
            ");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill GroupId từ Category (string) cũ trước khi cột này bị xóa; huy hiệu nào có
            // Category không khớp nhóm nào (không nên xảy ra) rơi về nhóm đầu tiên (Học tập).
            migrationBuilder.Sql(@"
                UPDATE [Achievements]
                SET [GroupId] = COALESCE(
                    (SELECT [Id] FROM [AchievementGroups] WHERE [Code] = [Achievements].[Category]),
                    (SELECT MIN([Id]) FROM [AchievementGroups])
                );
            ");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Achievements");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_GroupId",
                table: "Achievements",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Achievements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE [Achievements]
                SET [Category] = COALESCE(
                    (SELECT [Code] FROM [AchievementGroups] WHERE [Id] = [Achievements].[GroupId]),
                    'HocTap'
                );
            ");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_GroupId",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Achievements");

            migrationBuilder.DropTable(
                name: "AchievementGroups");
        }
    }
}
