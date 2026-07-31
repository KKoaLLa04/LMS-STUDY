# Checklist các tính năng/vấn đề cần hoàn thiện trước khi Go-live

File này tổng hợp từ một đợt rà soát toàn bộ codebase (Backend ASP.NET Core + Frontend Angular)
ngày 2026-07-30, nhằm liệt kê những gì **chưa sẵn sàng để chạy production**. Khác với
[backend-chua-co-fe.md](backend-chua-co-fe.md) (chỉ theo dõi việc BE có mà FE chưa nối), file
này bao quát rộng hơn: bug thực tế, dữ liệu giả/mock còn sót trong luồng chính, lỗ hổng bảo mật,
và các tính năng nghiệp vụ còn thiếu hẳn (chưa có cả BE lẫn FE).

---

> **Cập nhật 2026-07-30:** đã xử lý các mục 3, 4, 5, 7 (xem ghi chú "ĐÃ XỬ LÝ" trong từng mục).

## 🔴 Phải sửa trước khi go-live (ảnh hưởng người dùng thật / bảo mật)

1. **File cấu hình production của FE vẫn trỏ về localhost**
   `Frontend/src/environments/environment.ts` (`production: true`) có
   `apiBaseUrl: 'http://localhost:5100/api'` — giống hệt file dev. Build production hiện tại sẽ
   gọi API về `localhost` từ trình duyệt người dùng → **không hoạt động được**. Cũng đang dùng
   `http://` chứ chưa phải `https://`.

2. **CORS hardcode chỉ cho phép `http://localhost:4200`**
   `Backend/Program.cs:70-78, 223` — chưa đọc danh sách origin từ config theo môi trường. Deploy
   lên domain thật sẽ bị chặn toàn bộ request từ trình duyệt cho tới khi sửa.

3. ~~**Trang chủ (Homepage) 100% dữ liệu mock**~~ — ✅ **ĐÃ XỬ LÝ (trừ Testimonial, giữ mock theo
   chủ đích)**
   `Frontend/src/app/clients/pages/home/home.component.ts` trước đây dùng toàn bộ `MOCK_*` tĩnh.
   Đã nối vào dữ liệu thật:
   - **Khóa học nổi bật**: lấy từ `ClientCourseService.getCourses()`, lọc `featured === true`.
   - **Danh mục**: đếm số khóa học thật theo từng danh mục (từ danh sách khóa học đã fetch); icon/
     màu là lựa chọn trình bày thuần túy tra theo tên danh mục (backend không lưu icon), có icon
     mặc định cho danh mục chưa có trong bảng tra.
   - **Giáo viên nổi bật**: gọi `PublicUserService.getTeachers()` (`GET /api/users/teachers/public`,
     endpoint có sẵn từ trước) lấy tên/avatar thật; môn dạy & rating vẫn dùng phép suy diễn ổn định
     theo id (`buildTeacherRecord` trong `clients/pages/teachers/teachers.data.ts`) — dùng lại
     đúng logic đã áp dụng ở trang Giáo viên để nhất quán, backend chưa track 2 trường này (xem
     mục 12 bên dưới).
   - **Số liệu thống kê**: thêm endpoint mới `GET /api/platformstats`
     (`Backend/Controllers/PlatformStatsController.cs` → `PlatformStatsService.cs`) trả về số học
     viên/giáo viên/khóa học Published thật (đếm từ bảng `Users`/`Courses`) + tổng giờ nội dung
     (tính từ `Lessons.DurationMinutes`). Không lộ danh sách/PII, chỉ trả số đếm.
   - **Testimonial**: vẫn giữ `MOCK_TESTIMONIALS` — theo quyết định trước đó (chưa có bảng
     Testimonial ở backend, ưu tiên phần có dữ liệu thật trước).

4. ~~**Cờ `purchased` bị hardcode `false` cho mọi khóa học**~~ — ✅ **ĐÃ XỬ LÝ**
   Trước đây `Frontend/src/app/clients/utils/map-course-dto.util.ts:65,80` hardcode `false` dù đã
   có API thật `GET /api/enrollments/me`. Đã thêm `Frontend/src/app/clients/services/enrollment.service.ts`
   (gọi `GET /api/enrollments/me`) và nối vào `ClientCourseService.getCourses()`/`getCourseById()`
   (`forkJoin` với danh sách khóa học đã ghi danh) để `mapCourseListItemToCourse`/
   `mapCourseDetailToCourse` tính `purchased` thật theo user đang đăng nhập thay vì hardcode.

5. ~~**`rating`, `ratingCount`, `studentsCount` hardcode = 0**~~ — ✅ **ĐÃ XỬ LÝ**
   `studentsCount` nay tính thật từ số `Enrollment` theo khóa học (`CourseService.cs`, subquery
   giống cách `LessonsCount` đang tính). `rating`/`ratingCount` trước đây hoàn toàn không có dữ
   liệu backing (không có bảng Review nào) — đã xây mới:
   - Model `Backend/Models/CourseReview.cs` + migration `20260730042134_AddCourseReviews`
     (bảng `CourseReviews`, 1 học sinh chỉ đánh giá 1 lần/khóa học, unique index).
   - `Backend/Services/CourseReviewService.cs` + `Backend/Controllers/CourseReviewsController.cs`
     (`api/coursereviews`): tổng hợp điểm trung bình + phân bố sao, danh sách đánh giá phân trang,
     gửi/sửa đánh giá (chỉ học sinh **đã ghi danh** khóa học mới được đánh giá — giống "verified
     purchase"), xóa đánh giá (chủ sở hữu hoặc Admin).
   - `CourseListItemDto`/`CourseDetailDto` nay có `StudentsCount`/`Rating`/`RatingCount` tính thật.
   - FE: tab "Đánh giá" ở `clients/pages/course-detail/` nay gọi API thật (tổng hợp + danh sách),
     có form viết/sửa/xóa đánh giá (chỉ hiện khi đã ghi danh khóa học).

6. **Tài khoản admin mặc định `admin/admin123` được seed tự động**
   `Backend/Program.cs:201-212` — tạo sẵn nếu DB chưa có admin nào. Đây là credential mặc định
   đã biết công khai, **bắt buộc phải đổi/khóa trước khi golive** nếu không sẽ là cửa hậu.

7. ~~**Swagger UI bật vô điều kiện, không giới hạn theo môi trường**~~ — ✅ **ĐÃ XỬ LÝ**
   `Backend/Program.cs` nay bọc `app.UseSwagger()`/`app.UseSwaggerUI()` trong
   `if (app.Environment.IsDevelopment())` — Swagger chỉ khả dụng ở môi trường Development.

---

## 🟠 Phải quyết định trước khi launch (tính năng nghiệp vụ cốt lõi còn thiếu hẳn)

8. **Chưa có cổng thanh toán (Payment/Billing)**
   Không tìm thấy bất kỳ controller/service/model nào liên quan Payment, Order, Transaction,
   VNPay, Momo, Stripe (chỉ có `PointTransaction` — hệ điểm gamification, không liên quan tiền).
   `Course.Price` được lưu và hiển thị nhưng `EnrollmentService.EnrollAsync`
   (`Backend/Services/EnrollmentService.cs:21-51`) **không kiểm tra giá/thanh toán gì cả** — bất
   kỳ user đăng nhập nào cũng tự ghi danh miễn phí vào bất kỳ khóa học nào, kể cả khóa trả phí.
   Nếu mô hình kinh doanh cần thu tiền thật, phần này **chưa được xây dựng, không phải chỉ đang
   stub**.

9. **Chưa có hệ thống Email/SMS/Notification nào**
   Grep `Notification` trong Backend → 0 kết quả. Không có SmtpClient/MailKit/Twilio/SendGrid.
   Hệ quả: không có email chào mừng, không có thông báo khi mở khóa huy hiệu / có người trả lời
   thảo luận / giáo viên phản hồi — mọi thứ chỉ cập nhật khi người dùng tự mở trang lên xem
   (pull-only).
   > **Cập nhật:** riêng "quên mật khẩu" **đã có đường vòng** — Admin đặt lại mật khẩu cho bất kỳ
   > user nào ngay qua form Edit User có sẵn (`UpdateUserDto.Password` optional, không cần code
   > mới). Chưa xây luồng tự phục vụ qua email — cần bạn cung cấp tài khoản SMTP thật (Gmail app
   > password/SendGrid...) mới triển khai được, để dành khi có thông tin đó.

10. **Virtual Classrooms + Chat: Backend làm xong hoàn chỉnh nhưng Frontend chưa có gì**
    Grep `classroom|meeting|zoom|VirtualClassroom` trong `Frontend/src` → 0 kết quả. Trong khi
    `ChatChannelsController` và `VirtualClassroomsController` đã có đầy đủ CRUD, tin nhắn, chuyển
    trạng thái ở BE nhưng **không có route/service/component nào ở Angular**, và cả hai đang khóa
    `[Authorize(Roles="Admin")]` không có override cho học sinh/giáo viên (khác với Discussion
    Forum đã có override đọc/like). Đây là khoảng trống lớn hơn những gì
    [backend-chua-co-fe.md](backend-chua-co-fe.md) đang ghi nhận — file đó nói "không còn mục
    nào chưa có FE" nhưng Virtual Classrooms/Chat chính là trường hợp đó, chưa được liệt kê.
    Ngoài ra khi Zoom/GoogleMeet chưa bật (`MeetingProviders:Zoom:Enabled=false` — mặc định hiện
    tại), hệ thống dùng `FakeMeetingProviderService` sinh link giả
    `https://zoom.us/j/FAKE-XXXXXXXXX` — link chết, cần bật provider thật trước golive nếu tính
    năng này được đưa ra dùng.

---

## 🟡 Nên sửa (toàn vẹn dữ liệu / độ tin cậy)

> **Cập nhật 2026-07-30:** đã xử lý các mục 11, 12, 13, 14, 16 (xem ghi chú "ĐÃ XỬ LÝ"). Mục 15
> chỉ còn thiếu luồng tự phục vụ qua email — xem ghi chú cập nhật ở mục 9 phía trên.

11. ~~**Trang hồ sơ học sinh: "khóa học đang học" là dữ liệu tĩnh giả**~~ — ✅ **ĐÃ XỬ LÝ**
    `EnrollmentDto` nay có thêm `TotalLessons`/`CompletedLessons`/`ProgressPercent`, tính thật
    trong `EnrollmentService.GetMyEnrollmentsAsync` (cùng cách đếm completed/total lesson với
    `AchievementEvaluationService.ComputeCompletedCourseCountAsync`). FE
    (`student-profile.component.ts`) nay gọi `EnrollmentService.getMyEnrollments()`, lọc khóa học
    đã ghi danh có `0 < progress < 100`, hiển thị tối đa 4 khóa gần nhất — có empty state thật khi
    học sinh chưa học dở khóa nào, không còn danh sách tĩnh dùng chung cho mọi học sinh.

12. ~~**Trang chi tiết giáo viên: rating/số năm kinh nghiệm/môn dạy vẫn là dữ liệu suy diễn**~~ —
    ✅ **ĐÃ XỬ LÝ**
    Thêm 3 cột thật vào `User`: `Subject`, `ExperienceYears`, `Bio` (chỉ có ý nghĩa khi
    Role=Teacher), admin nhập qua form Edit User có sẵn
    (`features/users/pages/user-list/`). `PublicUserDto`/`PublicUser` (FE) trả kèm 3 trường này.
    Rating/danh sách khóa học/số học viên của giáo viên nay tính thật từ `Course` khớp theo tên
    giáo viên (`Course.Teacher === PublicUser.fullName`, không có FK) — xem
    `coursesOfTeacher`/`buildTeacherRecord` trong `clients/pages/teachers/teachers.data.ts`, dùng
    chung cho cả 3 nơi: trang chi tiết giáo viên, trang danh sách giáo viên (lọc theo môn dạy thật
    thay vì danh sách môn cố định), và khối "Giáo viên nổi bật" ở Trang chủ. Đánh giá học viên ở
    trang chi tiết giáo viên nay lấy thật từ `CourseReview` của các khóa học giáo viên đó dạy.

13. ~~**Bảng xếp hạng: mũi tên biến động hạng & khối/lớp luôn để trống**~~ — ✅ **ĐÃ XỬ LÝ**
    Thêm cột thật `User.KhoiHocId` (chỉ có ý nghĩa khi Role=Student), admin gán qua form Edit User
    (dropdown khối lấy từ `GET /api/khoihocs`, đã mở cho mọi user đăng nhập). Biến động hạng
    (`PreviousRank`) được `RankingService` **tính lại trực tiếp từ lịch sử `PointTransactions`**
    ở cửa sổ thời gian kỳ trước liền kề (tuần trước/tháng trước, hoặc 7 ngày trước với "toàn thời
    gian") — **không cần bảng snapshot lịch sử riêng** (đơn giản hơn phương án ban đầu, không thêm
    job nền). `RankingController` nay nhận thêm `khoiHocId` để lọc bảng xếp hạng theo khối; bộ lọc
    khối ở FE (`leaderboard.component.ts`) đã hoạt động thật (trước đây chỉ là chip tĩnh không lọc
    gì cả).

14. ~~**Huy hiệu (Achievement) có thể fallback về mock khi API trả rỗng**~~ — ✅ **ĐÃ XỬ LÝ**
    `achievement.service.ts`: bỏ hoàn toàn fallback sang `MOCK_BADGES` (cả khi API trả rỗng lẫn
    khi lỗi) — trả về mảng rỗng thật, có empty state ở UI (`achievements.component.html` đã thêm
    `@if` bảo vệ `latestBadge()` có thể `undefined`). `student-profile.component.ts` và
    `achievements.component.ts` cũng bỏ seed `MOCK_BADGES` làm giá trị khởi tạo — không còn tình
    huống hiển thị huy hiệu "đã mở khóa" giả trong lúc chờ API trả về.

15. **Không có luồng "Quên mật khẩu" tự phục vụ qua email** — xem ghi chú cập nhật ở mục 9.

16. ~~**Hai khái niệm "unlocked" song song cho Achievement**~~ — ✅ **ĐÃ XỬ LÝ**
    Xác nhận `Achievement.IsUnlocked`/`ProgressPercent` bị bỏ trống vĩnh viễn (không có chỗ nào
    từng ghi giá trị) và không được UI admin/học sinh nào thực sự dùng — đã **xóa hẳn 2 cột này**
    khỏi `Achievement` (model, DTO, migration `AddUserProfileGradeFields_DropAchievementUnlockFlags`)
    và khỏi model/mapper phía FE. `UserAchievement` giờ là nguồn dữ liệu duy nhất cho trạng thái mở
    khóa huy hiệu, không còn 2 khái niệm song song gây nhầm lẫn.

---

## 🟢 Mức độ thấp / dọn dẹp kỹ thuật

17. **Chưa có test tự động** — Backend chỉ có 1 `.csproj` duy nhất (không có test project); FE
    không có file `*.spec.ts` nào. Không có coverage ở cả hai phía.

18. **Lưu file trên đĩa cục bộ (`wwwroot/uploads`)** — `Backend/Services/UploadService.cs`. Chỉ
    ổn với deploy 1 instance có đĩa persistent; sẽ mất file khi redeploy/container restart, và
    không hoạt động đúng nếu chạy nhiều instance/load-balancing. Chưa tích hợp cloud storage
    (S3/Azure Blob...).

19. **Không có rate limiting** ở bất kỳ endpoint nào (kể cả login) — dễ bị brute-force/spam.

20. **JWT lưu ở `localStorage`** (`Frontend/src/app/core/auth/auth.service.ts:19-20,64-65,82-84`)
    — rủi ro bị đánh cắp qua XSS tiêu chuẩn; chưa dùng cookie `httpOnly`, chưa thấy CSP header
    trong `index.html`.

21. **Không có middleware xử lý exception toàn cục** và không phân nhánh dev/prod trong pipeline
    HTTP (`Program.cs` không có `UseExceptionHandler`) — lỗi trả về chưa được chuẩn hóa/ẩn chi
    tiết nhạy cảm.

22. **Còn sót `WeatherForecastController.cs`** — controller scaffold mặc định của ASP.NET Core,
    nên xóa cho gọn trước khi golive (không hại nhưng không cần thiết).

---

## Điểm tích cực (không cần sửa)

- `Jwt:Key` không hardcode trong `appsettings.json` — ứng dụng **throw lỗi khi khởi động** nếu
  thiếu, buộc phải cấu hình qua user-secrets/env (`Program.cs:154-158`). Đúng chuẩn.
- Mật khẩu được hash bằng BCrypt.
- `UseHttpsRedirection` đã có sẵn.
- Upload file có validate đuôi file + giới hạn dung lượng (dù chỉ theo phần mở rộng, chưa sniff
  nội dung/MIME), và chỉ Admin mới upload được nên rủi ro bị giới hạn.
