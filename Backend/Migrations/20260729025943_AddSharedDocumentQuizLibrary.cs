using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedDocumentQuizLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Bảng Document/Quiz dùng chung — mới, độc lập với Lesson ──────────────────
            // LegacyLessonId là cột tạm CHỈ dùng để backfill dữ liệu từ Lesson cũ (dò lại Document/
            // Quiz nào được sinh ra từ Lesson nào), không map vào EF model, bị xóa ở cuối migration.
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LegacyLessonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LegacyLessonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            // ── 2. Cột liên kết mới trên Lessons (nullable — nhiều Lesson có thể trỏ chung 1 Document/Quiz) ──
            migrationBuilder.AddColumn<int>(
                name: "DocumentId",
                table: "Lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "Lessons",
                type: "int",
                nullable: true);

            // ── 3. Backfill: mỗi Lesson Document/Quiz hiện có → 1 Document/Quiz mới tương ứng ──
            migrationBuilder.Sql(@"
                INSERT INTO Documents (Title, Content, FileUrl, CreatedAt, IsDeleted, LegacyLessonId)
                SELECT Title, Content, DocumentUrl, GETUTCDATE(), 0, Id
                FROM Lessons WHERE LessonType = 'Document';
            ");

            migrationBuilder.Sql(@"
                INSERT INTO Quizzes (Title, CreatedAt, IsDeleted, LegacyLessonId)
                SELECT Title, GETUTCDATE(), 0, Id
                FROM Lessons WHERE LessonType = 'Quiz';
            ");

            // ── 4. Trỏ Lesson.DocumentId/QuizId về Document/Quiz vừa backfill ──────────────
            migrationBuilder.Sql(@"
                UPDATE l SET l.DocumentId = d.Id
                FROM Lessons l JOIN Documents d ON d.LegacyLessonId = l.Id
                WHERE l.LessonType = 'Document';
            ");

            migrationBuilder.Sql(@"
                UPDATE l SET l.QuizId = q.Id
                FROM Lessons l JOIN Quizzes q ON q.LegacyLessonId = l.Id
                WHERE l.LessonType = 'Quiz';
            ");

            // ── 5. QuizQuestions: đổi LessonId → QuizId (giữ tạm giá trị LessonId cũ), rồi map
            // lại giá trị đó sang Quiz.Id tương ứng qua LegacyLessonId ─────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Lessons_LessonId",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "QuizQuestions",
                newName: "QuizId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizQuestions_LessonId",
                table: "QuizQuestions",
                newName: "IX_QuizQuestions_QuizId");

            migrationBuilder.Sql(@"
                UPDATE qq SET qq.QuizId = q.Id
                FROM QuizQuestions qq JOIN Quizzes q ON q.LegacyLessonId = qq.QuizId;
            ");

            // ── 6. QuizAttempts: backfill QuizId từ LessonId cũ TRƯỚC khi xóa cột LessonId ──
            migrationBuilder.AddColumn<int>(
                name: "QuizId",
                table: "QuizAttempts",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE qa SET qa.QuizId = q.Id
                FROM QuizAttempts qa JOIN Quizzes q ON q.LegacyLessonId = qa.LessonId;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Lessons_LessonId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_LessonId",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "QuizAttempts");

            // ── 7. Dọn cột tạm — chỉ phục vụ backfill, không map vào model EF ─────────────
            migrationBuilder.DropColumn(name: "LegacyLessonId", table: "Documents");
            migrationBuilder.DropColumn(name: "LegacyLessonId", table: "Quizzes");

            // ── 8. Index + Foreign key cho các cột mới ─────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_DocumentId",
                table: "Lessons",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_QuizId",
                table: "Lessons",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Documents_DocumentId",
                table: "Lessons",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Quizzes_QuizId",
                table: "Lessons",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Quizzes_QuizId",
                table: "QuizAttempts",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Quizzes_QuizId",
                table: "QuizQuestions",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        // Down() khôi phục lại đúng SCHEMA cũ, nhưng KHÔNG khôi phục lại dữ liệu Document/Quiz đã
        // backfill (một khi có Document/Quiz tạo mới độc lập không xuất phát từ Lesson nào, việc
        // "trả ngược" là không thể xác định — xem ghi chú rủi ro trong kế hoạch triển khai). Chỉ
        // nên chạy Down() trên môi trường dev/test, không chạy trên DB production đã có dữ liệu thật.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Documents_DocumentId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Quizzes_QuizId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Quizzes_QuizId",
                table: "QuizAttempts");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizQuestions_Quizzes_QuizId",
                table: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropIndex(
                name: "IX_QuizAttempts_QuizId",
                table: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_DocumentId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_QuizId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "DocumentId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "QuizId",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "QuizId",
                table: "QuizQuestions",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizQuestions_QuizId",
                table: "QuizQuestions",
                newName: "IX_QuizQuestions_LessonId");

            migrationBuilder.AddColumn<int>(
                name: "LessonId",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_LessonId",
                table: "QuizAttempts",
                column: "LessonId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Lessons_LessonId",
                table: "QuizAttempts",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizQuestions_Lessons_LessonId",
                table: "QuizQuestions",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
