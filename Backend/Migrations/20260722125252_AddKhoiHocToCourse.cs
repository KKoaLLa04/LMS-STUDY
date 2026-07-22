using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKhoiHocToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KhoiHocId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_KhoiHocId",
                table: "Courses",
                column: "KhoiHocId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_KhoiHocs_KhoiHocId",
                table: "Courses",
                column: "KhoiHocId",
                principalTable: "KhoiHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_KhoiHocs_KhoiHocId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_KhoiHocId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "KhoiHocId",
                table: "Courses");
        }
    }
}
