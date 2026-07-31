# Backend đã có nhưng Frontend chưa có

File này liệt kê các API/tính năng Backend đã code xong (có thể test qua Swagger) nhưng
chưa có giao diện Frontend tương ứng — để bổ sung dần khi hoàn thiện dự án.

> Cập nhật: các mục Follow, Like, Mở khóa huy hiệu thủ công và Discussion Forum bên dưới
> đã có FE (xem chi tiết trong từng mục). Không còn mục nào "chưa có FE" tại thời điểm này —
> file được giữ lại làm nhật ký cho những tính năng backend tiếp theo.

---

## 1. Follow (Theo dõi user) — ĐÃ CÓ FE

- `Backend/Controllers/FollowsController.cs`: `POST/DELETE /api/follows/{userId}`,
  `GET /api/follows/{userId}/status` (mới thêm — trả về `isFollowing`/`followersCount`/`followingCount`).
- FE:
  - `Frontend/src/app/shared/services/follow.service.ts` — service dùng chung.
  - Nút "Theo dõi" trên **bảng xếp hạng** (`clients/pages/leaderboard/`) — theo dõi học sinh khác
    ngay từ danh sách xếp hạng (`RankingEntryDto` giờ có thêm `IsFollowing`, tính 1 lần cho cả
    trang ở `RankingService` thay vì N truy vấn/dòng).
  - Nút "Theo dõi" + số người theo dõi trên **trang chi tiết giáo viên**
    (`clients/pages/teacher-detail/`) — trang này trước đây dùng dữ liệu mock hoàn toàn, nay đã
    nối với API thật (`GET /api/users/teachers/public`, `GET /api/users/public/{id}` — 2 endpoint
    công khai mới thêm ở `UsersController`, trả `PublicUserDto` tối giản không lộ email/SĐT).
    Rating/môn dạy/số năm kinh nghiệm vẫn là dữ liệu suy diễn (backend chưa track), chỉ tên/avatar/
    giới tính là thật.

## 2. Like bài viết thảo luận (Discussion Post) — ĐÃ CÓ FE

- `Backend/Controllers/VirtualClassrooms/DiscussionForumsController.cs`:
  `POST/DELETE /api/discussionforums/{id}/like`. Đồng thời đã mở `GET by-course/{courseId}` và
  `GET {id}` cho mọi user đã đăng nhập (trước đây cả class bị giới hạn `Admin` nên học sinh không
  xem được bài viết) — đăng bài/trả lời vẫn giới hạn Admin như cũ.
- DTOs (`DiscussionPostListItemDto`, `DiscussionPostResponseDto`) nay có thêm `LikeCount` và
  `IsLikedByCurrentUser` để FE hiển thị số lượt thích + trạng thái đã thích hay chưa.
- FE:
  - `Frontend/src/app/shared/services/discussion-forum.service.ts` +
    `shared/models/discussion-post.model.ts` — dùng chung cho cả 2 phía dưới.
  - **Phía học sinh**: tab "Thảo luận" mới trong `clients/pages/course-detail/` (component
    `clients/components/discussion-panel/`) — xem danh sách bài viết theo khóa học, mở rộng xem
    trả lời, và nút Thích/Bỏ thích cho cả bài viết lẫn từng trả lời. Chỉ xem + thích, không đăng
    bài (đăng bài vẫn giới hạn Admin).
  - **Phía Admin**: trang quản lý mới `features/discussion-forums/` (route `/discussion-forums`,
    đã thêm vào sidebar) — tạo/sửa/xóa bài viết, xem chi tiết kèm trả lời, trả lời bài viết, xóa
    từng trả lời.

**Lưu ý còn tồn tại (không phải do đợt này tạo ra):** đăng bài/trả lời vẫn giới hạn
`[Authorize(Roles = "Admin")]` ở `DiscussionForumsController` — học sinh tự đăng bài chưa được
mở. Vì vậy điều kiện huy hiệu `CommentCount` vẫn chỉ thực sự có ý nghĩa khi luồng học sinh tự
đăng bài được mở ra sau này.

## 3. Mở khóa huy hiệu thủ công cho học sinh (Admin) — ĐÃ CÓ FE

- `POST /api/achievements/{id}/unlock/{userId}` (có sẵn từ trước, không phải đợt này thêm).
- FE: nút "Mở khóa huy hiệu thủ công" (icon huy hiệu) trên mỗi dòng học sinh ở trang
  `features/users/pages/user-list/` (route `/students`) — mở modal chọn huy hiệu từ dropdown rồi
  xác nhận. Chỉ hiển thị khi xem danh sách học sinh (không hiển thị ở danh sách giáo viên).

## 4. Điều kiện huy hiệu `ReceivedLikeCount` / `FollowerCount` — nay đã có dữ liệu thật

Trước đây 2 loại điều kiện này luôn tính ra 0 vì không có giao diện thật để user bấm Like/Follow.
Nay cả hai đã có FE (mục 1, 2 ở trên) nên khi học sinh theo dõi nhau hoặc thích bài viết, các huy
hiệu dùng điều kiện `FollowerCount`/`ReceivedLikeCount` có thể tự động mở khóa như thiết kế.
