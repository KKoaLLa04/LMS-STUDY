using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKhoiHocForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_KhoiHocs_KhoiHocId",
                table: "Courses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Courses_KhoiHocs_KhoiHocId",
                table: "Courses",
                column: "KhoiHocId",
                principalTable: "KhoiHocs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
