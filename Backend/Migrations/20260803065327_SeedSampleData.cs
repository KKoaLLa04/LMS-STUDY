using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dữ liệu mẫu cho học sinh tiểu học (Khối 1-5): khối học, môn học, giảng viên, học
            // sinh, nhóm huy hiệu/thành tích, tài liệu, bài quiz và khóa học (kèm chương/bài học).
            // Mật khẩu của mọi tài khoản mẫu (giảng viên/học sinh): password123
            // Dùng DO block để lấy Id vừa insert (hoặc Id đã có sẵn nếu trùng khóa duy nhất)
            // và dùng lại cho các bảng con — tránh phải đoán trước Id, an toàn khi migration
            // được áp dụng lên DB đã có dữ liệu (Username/Code trùng thì bỏ qua, không lỗi).
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    v_k1 integer; v_k2 integer; v_k3 integer; v_k4 integer; v_k5 integer;
                    v_cat_toan integer; v_cat_tviet integer; v_cat_anh integer; v_cat_daoduc integer; v_cat_tnxh integer;
                    v_gv_toan integer; v_gv_van integer; v_gv_anh integer;
                    v_hs_duc integer; v_hs_hoa integer; v_hs_khoa integer; v_hs_linh integer; v_hs_minh integer;
                    v_grp_hoctap integer; v_grp_chuyencan integer;
                    v_doc1 integer; v_doc2 integer; v_doc3 integer;
                    v_quiz1 integer; v_quiz2 integer;
                    v_q1_1 integer; v_q1_2 integer; v_q2_1 integer; v_q2_2 integer;
                    v_course1 integer; v_course2 integer; v_course3 integer; v_course4 integer;
                    v_sec1 integer; v_sec2 integer; v_sec3 integer; v_sec4 integer;
                BEGIN
                    -- Khối học (Tiểu học: Khối 1 - Khối 5)
                    INSERT INTO "KhoiHocs" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Khối 1','K1',1,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_k1 FROM "KhoiHocs" WHERE "Code" = 'K1';

                    INSERT INTO "KhoiHocs" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Khối 2','K2',2,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_k2 FROM "KhoiHocs" WHERE "Code" = 'K2';

                    INSERT INTO "KhoiHocs" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Khối 3','K3',3,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_k3 FROM "KhoiHocs" WHERE "Code" = 'K3';

                    INSERT INTO "KhoiHocs" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Khối 4','K4',4,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_k4 FROM "KhoiHocs" WHERE "Code" = 'K4';

                    INSERT INTO "KhoiHocs" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Khối 5','K5',5,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_k5 FROM "KhoiHocs" WHERE "Code" = 'K5';

                    -- Môn học (chương trình tiểu học)
                    INSERT INTO "CourseCategories" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Toán học','TOAN',1,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_cat_toan FROM "CourseCategories" WHERE "Code" = 'TOAN';

                    INSERT INTO "CourseCategories" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Tiếng Việt','TIENGVIET',2,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_cat_tviet FROM "CourseCategories" WHERE "Code" = 'TIENGVIET';

                    INSERT INTO "CourseCategories" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Tiếng Anh','TIENGANH',3,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_cat_anh FROM "CourseCategories" WHERE "Code" = 'TIENGANH';

                    INSERT INTO "CourseCategories" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Đạo đức','DAODUC',4,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_cat_daoduc FROM "CourseCategories" WHERE "Code" = 'DAODUC';

                    INSERT INTO "CourseCategories" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Tự nhiên và Xã hội','TNXH',5,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_cat_tnxh FROM "CourseCategories" WHERE "Code" = 'TNXH';

                    -- Giảng viên
                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","Subject","ExperienceYears","Bio","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('gv.toan','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Teacher','gv.toan@lms-study.vn','Nguyễn Văn An','0901000001',0,'Toán học',8,'Giáo viên Toán tiểu học với 8 năm kinh nghiệm giảng dạy.',false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_gv_toan FROM "Users" WHERE "Username" = 'gv.toan';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","Subject","ExperienceYears","Bio","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('gv.van','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Teacher','gv.van@lms-study.vn','Trần Thị Bình','0901000002',0,'Tiếng Việt',6,'Giáo viên Tiếng Việt tiểu học, chuyên rèn kỹ năng đọc viết cho học sinh nhỏ tuổi.',false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_gv_van FROM "Users" WHERE "Username" = 'gv.van';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","Subject","ExperienceYears","Bio","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('gv.anh','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Teacher','gv.anh@lms-study.vn','Lê Thị Chi','0901000003',0,'Tiếng Anh',5,'Giáo viên Tiếng Anh tiểu học, dạy Tiếng Anh qua trò chơi và hình ảnh.',false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_gv_anh FROM "Users" WHERE "Username" = 'gv.anh';

                    -- Học sinh (mỗi em một khối, từ Khối 1 đến Khối 5)
                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","KhoiHocId","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('hs.duc','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Student','hs.duc@lms-study.vn','Phạm Văn Đức','0902000001',0,v_k1,false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_hs_duc FROM "Users" WHERE "Username" = 'hs.duc';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","KhoiHocId","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('hs.hoa','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Student','hs.hoa@lms-study.vn','Hoàng Thị Hoa','0902000002',0,v_k2,false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_hs_hoa FROM "Users" WHERE "Username" = 'hs.hoa';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","KhoiHocId","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('hs.khoa','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Student','hs.khoa@lms-study.vn','Đặng Văn Khoa','0902000003',0,v_k3,false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_hs_khoa FROM "Users" WHERE "Username" = 'hs.khoa';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","KhoiHocId","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('hs.linh','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Student','hs.linh@lms-study.vn','Vũ Thị Linh','0902000004',0,v_k4,false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_hs_linh FROM "Users" WHERE "Username" = 'hs.linh';

                    INSERT INTO "Users" ("Username","PasswordHash","Role","Email","FullName","Phone","Status","KhoiHocId","IsDeleted","LoginStreakCount","HomeworkStreakCount")
                    VALUES ('hs.minh','$2a$11$5/vsBPM7gHirJU2YrZN6seb9jvW5IaZEgaFeNFGgun0CNjBSubOOe','Student','hs.minh@lms-study.vn','Bùi Văn Minh','0902000005',0,v_k5,false,0,0)
                    ON CONFLICT ("Username") DO NOTHING;
                    SELECT "Id" INTO v_hs_minh FROM "Users" WHERE "Username" = 'hs.minh';

                    -- Nhóm huy hiệu
                    INSERT INTO "AchievementGroups" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Học tập','HOCTAP',1,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_grp_hoctap FROM "AchievementGroups" WHERE "Code" = 'HOCTAP';

                    INSERT INTO "AchievementGroups" ("Name","Code","OrderNumber","IsDeleted")
                    VALUES ('Chuyên cần','CHUYENCAN',2,false)
                    ON CONFLICT ("Code") WHERE "IsDeleted" = false DO NOTHING;
                    SELECT "Id" INTO v_grp_chuyencan FROM "AchievementGroups" WHERE "Code" = 'CHUYENCAN';

                    -- Thành tích
                    INSERT INTO "Achievements" ("Name","Description","GroupId","IconKey","OrderNumber","Points","IsDeleted")
                    VALUES ('Người mới bắt đầu','Hoàn thành bài học đầu tiên trên hệ thống.',v_grp_hoctap,'book-open',1,20,false);

                    INSERT INTO "Achievements" ("Name","Description","GroupId","IconKey","OrderNumber","Points","IsDeleted")
                    VALUES ('Học sinh chăm chỉ','Hoàn thành 10 bài học.',v_grp_hoctap,'flame',2,50,false);

                    INSERT INTO "Achievements" ("Name","Description","GroupId","IconKey","OrderNumber","Points","IsDeleted")
                    VALUES ('Bậc thầy Quiz','Đạt điểm tối đa trong một bài quiz.',v_grp_hoctap,'trophy',3,100,false);

                    INSERT INTO "Achievements" ("Name","Description","GroupId","IconKey","OrderNumber","Points","IsDeleted")
                    VALUES ('Chuyên cần 7 ngày','Đăng nhập liên tục 7 ngày.',v_grp_chuyencan,'calendar-check',1,30,false);

                    INSERT INTO "Achievements" ("Name","Description","GroupId","IconKey","OrderNumber","Points","IsDeleted")
                    VALUES ('Chuyên cần 30 ngày','Đăng nhập liên tục 30 ngày.',v_grp_chuyencan,'calendar-star',2,150,false);

                    -- Tài liệu
                    INSERT INTO "Documents" ("Title","Content","Status","IsDeleted")
                    VALUES ('Bảng chữ cái Tiếng Việt','Tổng hợp 29 chữ cái Tiếng Việt kèm cách phát âm, dùng cho học sinh lớp 1-2 luyện đọc.','SharedAndForLesson',false)
                    RETURNING "Id" INTO v_doc1;

                    INSERT INTO "Documents" ("Title","Content","Status","IsDeleted")
                    VALUES ('Bảng cộng trừ trong phạm vi 10','Tổng hợp các phép cộng, trừ cơ bản trong phạm vi 10 dành cho học sinh lớp 1.','Shared',false)
                    RETURNING "Id" INTO v_doc2;

                    INSERT INTO "Documents" ("Title","Content","Status","IsDeleted")
                    VALUES ('Từ vựng Tiếng Anh cho bé: Màu sắc và con vật','Danh sách từ vựng Tiếng Anh cơ bản về màu sắc và con vật, kèm hình ảnh minh họa.','Shared',false)
                    RETURNING "Id" INTO v_doc3;

                    -- Bài quiz: Tiếng Anh (màu sắc)
                    INSERT INTO "Quizzes" ("Title","Description","IsDeleted")
                    VALUES ('Ôn tập Tiếng Anh: Màu sắc','Bài kiểm tra nhanh từ vựng Tiếng Anh về màu sắc.',false)
                    RETURNING "Id" INTO v_quiz1;

                    INSERT INTO "QuizQuestions" ("QuizId","Text","OrderNumber","AllowMultipleAnswers","IsDeleted")
                    VALUES (v_quiz1,'Từ nào có nghĩa là màu đỏ?',1,false,false)
                    RETURNING "Id" INTO v_q1_1;
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_1,'Red',true,1);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_1,'Blue',false,2);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_1,'Green',false,3);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_1,'Yellow',false,4);

                    INSERT INTO "QuizQuestions" ("QuizId","Text","OrderNumber","AllowMultipleAnswers","IsDeleted")
                    VALUES (v_quiz1,'Từ nào có nghĩa là màu xanh dương?',2,false,false)
                    RETURNING "Id" INTO v_q1_2;
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_2,'Blue',true,1);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_2,'Red',false,2);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_2,'Black',false,3);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q1_2,'White',false,4);

                    -- Bài quiz: Toán (phép cộng trong phạm vi 10)
                    INSERT INTO "Quizzes" ("Title","Description","IsDeleted")
                    VALUES ('Ôn tập Toán: Phép cộng trong phạm vi 10','Bài kiểm tra nhanh phép cộng cơ bản cho học sinh lớp 1.',false)
                    RETURNING "Id" INTO v_quiz2;

                    INSERT INTO "QuizQuestions" ("QuizId","Text","OrderNumber","AllowMultipleAnswers","IsDeleted")
                    VALUES (v_quiz2,'2 + 3 = ?',1,false,false)
                    RETURNING "Id" INTO v_q2_1;
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_1,'5',true,1);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_1,'4',false,2);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_1,'6',false,3);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_1,'3',false,4);

                    INSERT INTO "QuizQuestions" ("QuizId","Text","OrderNumber","AllowMultipleAnswers","IsDeleted")
                    VALUES (v_quiz2,'4 + 4 = ?',2,false,false)
                    RETURNING "Id" INTO v_q2_2;
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_2,'8',true,1);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_2,'7',false,2);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_2,'9',false,3);
                    INSERT INTO "QuizOptions" ("QuestionId","Text","IsCorrect","OrderNumber") VALUES (v_q2_2,'6',false,4);

                    -- Khóa học
                    INSERT INTO "Courses" ("Title","Description","Teacher","Emoji","Price","Status","KhoiHocId","CategoryId","IsFeatured","IsDeleted")
                    VALUES ('Toán học lớp 1','Làm quen với các số và phép cộng, trừ trong phạm vi 10.','Nguyễn Văn An','🔢',0,'Published',v_k1,v_cat_toan,true,false)
                    RETURNING "Id" INTO v_course1;

                    INSERT INTO "Courses" ("Title","Description","Teacher","Emoji","Price","Status","KhoiHocId","CategoryId","IsFeatured","IsDeleted")
                    VALUES ('Tiếng Việt lớp 2','Luyện đọc, luyện viết chính tả và mở rộng vốn từ Tiếng Việt.','Trần Thị Bình','📚',0,'Published',v_k2,v_cat_tviet,false,false)
                    RETURNING "Id" INTO v_course2;

                    INSERT INTO "Courses" ("Title","Description","Teacher","Emoji","Price","Status","KhoiHocId","CategoryId","IsFeatured","IsDeleted")
                    VALUES ('Tiếng Anh cho bé lớp 3','Học Tiếng Anh qua màu sắc, con vật và các mẫu câu giao tiếp đơn giản.','Lê Thị Chi','🗣',199000,'Published',v_k3,v_cat_anh,false,false)
                    RETURNING "Id" INTO v_course3;

                    INSERT INTO "Courses" ("Title","Description","Teacher","Emoji","Price","Status","KhoiHocId","CategoryId","IsFeatured","IsDeleted")
                    VALUES ('Tự nhiên và Xã hội lớp 4','Khám phá thế giới tự nhiên và xã hội xung quanh em.','Nguyễn Văn An','🌱',0,'Upcoming',v_k4,v_cat_tnxh,false,false)
                    RETURNING "Id" INTO v_course4;

                    -- Chương + bài học
                    INSERT INTO "Sections" ("CourseId","Title","Position","IsDeleted")
                    VALUES (v_course1,'Chương 1: Làm quen với số',1,false) RETURNING "Id" INTO v_sec1;
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","IsDeleted")
                    VALUES (v_sec1,'Các số từ 1 đến 10','Video',1,10,false);
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","QuizId","IsDeleted")
                    VALUES (v_sec1,'Luyện tập phép cộng','Quiz',2,10,v_quiz2,false);

                    INSERT INTO "Sections" ("CourseId","Title","Position","IsDeleted")
                    VALUES (v_course2,'Chương 1: Bảng chữ cái và chính tả',1,false) RETURNING "Id" INTO v_sec2;
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","DocumentId","IsDeleted")
                    VALUES (v_sec2,'Ôn tập bảng chữ cái','Document',1,15,v_doc1,false);
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","IsDeleted")
                    VALUES (v_sec2,'Luyện viết chính tả','Video',2,15,false);

                    INSERT INTO "Sections" ("CourseId","Title","Position","IsDeleted")
                    VALUES (v_course3,'Chủ đề 1: Màu sắc',1,false) RETURNING "Id" INTO v_sec3;
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","IsDeleted")
                    VALUES (v_sec3,'Từ vựng màu sắc','Video',1,10,false);
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","QuizId","IsDeleted")
                    VALUES (v_sec3,'Luyện tập','Quiz',2,10,v_quiz1,false);

                    INSERT INTO "Sections" ("CourseId","Title","Position","IsDeleted")
                    VALUES (v_course4,'Chương 1: Con người và sức khỏe',1,false) RETURNING "Id" INTO v_sec4;
                    INSERT INTO "Lessons" ("SectionId","Title","LessonType","Position","DurationMinutes","IsDeleted")
                    VALUES (v_sec4,'Cơ thể em','Video',1,12,false);
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa theo thứ tự ngược: Course/Quiz xóa trước sẽ cascade xóa luôn Section/Lesson
            // và QuizQuestion/QuizOption tương ứng (xem cấu hình OnDelete trong AppDbContext).
            migrationBuilder.Sql("""
                DELETE FROM "Courses" WHERE "Title" IN (
                    'Toán học lớp 1','Tiếng Việt lớp 2','Tiếng Anh cho bé lớp 3','Tự nhiên và Xã hội lớp 4'
                );
                DELETE FROM "Quizzes" WHERE "Title" IN (
                    'Ôn tập Tiếng Anh: Màu sắc','Ôn tập Toán: Phép cộng trong phạm vi 10'
                );
                DELETE FROM "Documents" WHERE "Title" IN (
                    'Bảng chữ cái Tiếng Việt','Bảng cộng trừ trong phạm vi 10','Từ vựng Tiếng Anh cho bé: Màu sắc và con vật'
                );
                DELETE FROM "Achievements" WHERE "Name" IN (
                    'Người mới bắt đầu','Học sinh chăm chỉ','Bậc thầy Quiz','Chuyên cần 7 ngày','Chuyên cần 30 ngày'
                );
                DELETE FROM "AchievementGroups" WHERE "Code" IN ('HOCTAP','CHUYENCAN');
                DELETE FROM "Users" WHERE "Username" IN (
                    'gv.toan','gv.van','gv.anh','hs.duc','hs.hoa','hs.khoa','hs.linh','hs.minh'
                );
                DELETE FROM "CourseCategories" WHERE "Code" IN ('TOAN','TIENGVIET','TIENGANH','DAODUC','TNXH');
                DELETE FROM "KhoiHocs" WHERE "Code" IN ('K1','K2','K3','K4','K5');
                """);
        }
    }
}
