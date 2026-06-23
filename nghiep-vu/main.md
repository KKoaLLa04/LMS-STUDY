**********************************************************
******************** Tính năng Core **********************
**********************************************************
Các tính năng chính cần có trong dự án LMS:
1. Quản lý và lưu trứ khóa học (course Management)
    + Tải lên đa dạng tài liệu
    + Xây dựng lộ trình học (học hết bài 1 => đến bài 2) (cho config hệ thống cho phép nhảy bài để học)
2. Quản lý người dùng và phân quyền
    + Phân chia vai trò rõ ràng: Admin, Manager (quản lý đào tạo), Teacher/Instructor(giáo viên), Student/Learner (học viên)
    + Quản lý theo lớp/ phòng ban: gom nhóm học viên theo lớp học hoặc theo phòng ban hoặc theo khóa học
3. Tương tác và lớp học trực tuyến
    + Tích hợp phòng học trực tuyến: kết nối với zoom, google meet hoặc Ms Teams
    + Công cụ thảo luận (forum), kênh chat nội bộ (học viên đặt câu hỏi cho giảng viên hoặc thảo luận nhóm)
4. Kiểm tra và đánh giá chất lượng (Assessment & Quiz)
    Tính năng giúp đo lường mức độ hiểu bài của người học:
    + Đa dạng dạng câu hỏi: Trắc nghiệm (chọn 1 hoặc nhiều đáp án), tự luận, điền vào chỗ trống, nối từ
    + Ngân hàng đề thi (Question Bank): Tự động trộn đề, đảo câu hỏi và đáp án để chống gian lận.
    + Chấm điểm tự động: Trả kết quả và đáp án chi tiết ngay sau khi học viên nộp bài (đối với bài trắc nghiệm).
5. Báo cáo và thống kê chi tiết (Reporting & Analytics)
    + Báo cáo tiến độ: Biết được học viên đã học đến bài nào, dành bao nhiêu thời gian cho khóa học.
    + Thống kê điểm số: Biểu đồ trực quan về phổ điểm của lớp học, tỷ lệ hoàn thành khóa học, những câu hỏi nào học viên hay làm sai nhất.

6. Cấp chứng nhận và vinh danh
    + Tự động cung cấp chứng chỉ
    + Game hóa việc học: Tặng huy hiệu, tính điểm thưởng và xếp hạng


    Một hệ thống LMS tốt không chỉ đơn thuần là nơi chứa tài liệu, mà phải là một hệ sinh thái khép kín: Lưu trữ bài học -> Tương tác học tập -> Đánh giá kết quả -> Báo cáo tiến độ -> Cấp chứng nhận.

**********************************************************
******* Các tính năng nâng cao phát triển lâu dài: *******
**********************************************************

1. Ứng dụng trí tuệ nhân tạo
    + Gợi ý lộ trình cá nhân hóa (Adaptive Learning): Hệ thống dựa vào hành vi và kết quả bài test của người học để tự động đề xuất bài học tiếp theo. Nếu học viên làm sai phần "Hàm số", AI sẽ tự động gợi ý các bài giảng bổ trợ về phần này thay vì bắt học theo một lộ trình rập khuôn.
    + Trợ lý ảo AI Chatbot 24/7: Giải đáp ngay lập tức các thắc mắc của học viên về nội dung bài học hoặc kỹ thuật (Ví dụ: "Làm sao để nộp bài PDF?", "Công thức tính đạo hàm bài trước là gì?").
    + Tự động tạo nội dung (AI Course Builder): Hỗ trợ giảng viên quét một tài liệu dài và tự động xuất ra bộ câu hỏi trắc nghiệm, tóm tắt bài học hoặc slide bài giảng chỉ trong vài giây.

2. Giám sát thi cử thông minh (AI Proctoring) (CHỈ PHỤC VỤ KHI THI ONLINE => PENDING)

3. Thiết kế tối ưu cho Học tập siêu nhỏ (Microlearning & Mobile-first)
- Con người ngày nay có xu hướng mất tập trung rất nhanh, do đó LMS cần có các tính năng hỗ trợ học nhanh:
    + Học tập dạng "Tiktok" (Short-form video): Hỗ trợ các chuỗi bài học dạng video ngắn 3-5 phút, có thể vuốt để chuyển bài tiếp theo ngay trên điện thoại.
    + Ứng dụng di động đồng bộ (Native Mobile App): Không chỉ là lướt web trên điện thoại, hệ thống có app riêng hỗ trợ Học ngoại tuyến (Offline Mode) — cho phép tải bài học về máy khi có Wifi và học lại trên máy bay/xe bus, tiến độ sẽ tự đồng bộ khi có mạng trở lại. (TÍNH NĂNG KHÔNG CẦN THIẾT LẮM)

4. Hệ thống Tự động hóa quy trình (Automation Workflows)
    + Giao bài tự động theo điều kiện (Triggers): Ví dụ trong doanh nghiệp, ngay khi HR cập nhật một nhân sự mới lên hệ thống ở vị trí "Nhân viên Sales", LMS sẽ tự động thêm người đó vào khóa "Kỹ năng giao tiếp" và "Quy trình sản phẩm" mà không cần bấm tay.
    + Nhắc nhở thông minh theo hành vi: Tự động gửi email hoặc thông báo push trên app nếu học viên đã 5 ngày chưa đăng nhập, hoặc nhắc nhở trước 2 ngày khi sắp đến hạn nộp bài.

5. Phân tích dữ liệu dự đoán (Predictive Learning Analytics)
- Không chỉ dừng lại ở việc báo cáo xem ai đã hoàn thành bài học, các LMS cao cấp có khả năng:
    + Cảnh báo rủi ro (Risk Detection): Hệ thống phân tích tần suất đăng nhập, thời gian xem video ngắn, điểm số giảm dần để đưa ra cảnh báo: "Học viên A có 80% nguy cơ sẽ bỏ học giữa chừng". Từ đó, giảng viên có thể can thiệp hỗ trợ kịp thời.
    + Đo lường độ hiệu quả của học liệu: Báo cáo cho biết học viên thường bấm "Tạm dừng" hoặc "Tua lại" ở phút thứ mấy của video giảng dạy, giúp giảng viên biết đoạn đó nội dung đang bị quá khó hoặc quá nhàm chán để sửa đổi.

6. Thương mại hóa khóa học (E-Commerce tích hợp)
    + Cổng thanh toán tự động: Tích hợp quét mã QR MoMo, ZaloPay, VietQR ngân hàng. Học viên chuyển khoản xong là hệ thống tự động kích hoạt khóa học ngay lập tức.
    + Quản lý mã giảm giá (Coupon), Affiliate (Tiếp thị liên kết): Cho phép học viên chia sẻ link khóa học để nhận hoa hồng, giúp website tự động tăng trưởng doanh số.

YÊU CẦU TIÊN QUYẾT: cần có Hạ tầng Stream Video thông minh (Adaptive Bitrate Streaming)