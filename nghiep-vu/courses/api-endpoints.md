# 🔌 RESTful API Endpoints - Hệ thống Quản lý Khóa học

## Base URL
```
https://api.coursepro.com/v1
```

---

## 📚 **1. COURSES (Khóa học)**

### 1.1 Lấy danh sách khóa học
```http
GET /courses
```
**Query Parameters:**
- `page` (int, default=1): Trang
- `limit` (int, default=10): Số lượng trên trang
- `search` (string): Tìm kiếm theo tiêu đề
- `category` (UUID): Lọc theo danh mục
- `status` (enum): draft, published, archived
- `level` (enum): elementary, middle_school
- `sort` (string): created_at, updated_at, title

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "c123",
      "title": "Toán lớp 5",
      "description": "Khóa học toán cơ bản",
      "category": { "id": "cat1", "name": "Toán" },
      "instructor": { "id": "u1", "fullName": "Nguyễn Văn A" },
      "level": "elementary",
      "status": "published",
      "version": 1,
      "enrollmentCount": 45,
      "maxStudents": 50,
      "thumbnailUrl": "https://...",
      "createdAt": "2025-01-15T10:30:00Z",
      "publishedAt": "2025-01-20T10:30:00Z"
    }
  ],
  "pagination": {
    "total": 120,
    "page": 1,
    "limit": 10,
    "totalPages": 12
  }
}
```

---

### 1.2 Lấy chi tiết khóa học
```http
GET /courses/{courseId}
```

**Response (200 OK):**
```json
{
  "id": "c123",
  "title": "Toán lớp 5",
  "description": "...",
  "category": { "id": "cat1", "name": "Toán" },
  "instructor": { "id": "u1", "fullName": "Nguyễn Văn A", "email": "...@edu.vn" },
  "level": "elementary",
  "status": "published",
  "version": 1,
  "thumbnailUrl": "https://...",
  "duration": {
    "totalMinutes": 450,
    "totalChapters": 5,
    "totalLessons": 32
  },
  "enrollment": {
    "maxStudents": 50,
    "enrolledCount": 45,
    "openFor": "active"
  },
  "startDate": "2025-01-20",
  "endDate": "2025-03-31",
  "chapters": [
    {
      "id": "ch1",
      "title": "Chương 1: Số tự nhiên",
      "orderIndex": 1,
      "lessonCount": 6,
      "minCompletionPercentage": 80,
      "lessons": [
        {
          "id": "l1",
          "title": "Bài 1: Số tự nhiên là gì",
          "lessonType": "theory",
          "orderIndex": 1,
          "durationMinutes": 15,
          "videoUrl": "https://...",
          "thumbnailUrl": "https://...",
          "prerequisites": []
        }
      ]
    }
  ],
  "rating": {
    "average": 4.5,
    "count": 120
  },
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-18T14:20:00Z"
}
```

---

### 1.3 Tạo khóa học
```http
POST /courses
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Toán lớp 5",
  "description": "Khóa học Toán...",
  "categoryId": "cat1",
  "level": "elementary",
  "thumbnailUrl": "https://...",
  "maxStudents": 50,
  "startDate": "2025-01-20",
  "endDate": "2025-03-31"
}
```

**Response (201 Created):**
```json
{
  "id": "c123",
  "title": "Toán lớp 5",
  "status": "draft",
  "version": 1,
  "createdAt": "2025-01-15T10:30:00Z"
}
```

---

### 1.4 Cập nhật khóa học (Draft)
```http
PUT /courses/{courseId}
Authorization: Bearer {token}
```

**Request Body:** (tương tự như Create)

**Response (200 OK):**
```json
{
  "id": "c123",
  "title": "Toán lớp 5 - Cập nhật",
  "status": "draft",
  "updatedAt": "2025-01-18T14:20:00Z"
}
```

---

### 1.5 Công bố khóa học (Draft → Published)
```http
POST /courses/{courseId}/publish
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "notes": "Khóa học đã sẵn sàng công bố"
}
```

**Response (200 OK):**
```json
{
  "id": "c123",
  "status": "published",
  "version": 1,
  "publishedAt": "2025-01-20T10:30:00Z"
}
```

---

### 1.6 Lưu bản nháp khóa học
```http
POST /courses/{courseId}/save-draft
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "c123",
  "status": "draft",
  "version": 1,
  "savedAt": "2025-01-15T15:45:00Z"
}
```

---

## 📖 **2. CHAPTERS (Chương học)**

### 2.1 Lấy danh sách chương
```http
GET /courses/{courseId}/chapters
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "ch1",
      "title": "Chương 1: Số tự nhiên",
      "description": "...",
      "orderIndex": 1,
      "lessonCount": 6,
      "minCompletionPercentage": 80,
      "lessonsUrl": "/courses/c123/chapters/ch1/lessons"
    }
  ]
}
```

---

### 2.2 Tạo chương
```http
POST /courses/{courseId}/chapters
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Chương 1: Số tự nhiên",
  "description": "...",
  "orderIndex": 1,
  "minCompletionPercentage": 80,
  "passRequired": false
}
```

**Response (201 Created):**
```json
{
  "id": "ch1",
  "courseId": "c123",
  "title": "Chương 1: Số tự nhiên",
  "orderIndex": 1,
  "createdAt": "2025-01-15T10:30:00Z"
}
```

---

### 2.3 Cập nhật chương
```http
PUT /courses/{courseId}/chapters/{chapterId}
Authorization: Bearer {token}
```

---

### 2.4 Xóa chương
```http
DELETE /courses/{courseId}/chapters/{chapterId}
Authorization: Bearer {token}
```

---

## 📝 **3. LESSONS (Bài học)**

### 3.1 Lấy danh sách bài học
```http
GET /courses/{courseId}/chapters/{chapterId}/lessons
```

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "l1",
      "title": "Bài 1: Số tự nhiên là gì",
      "lessonType": "theory",
      "orderIndex": 1,
      "durationMinutes": 15,
      "videoUrl": "https://youtube.com/...",
      "thumbnailUrl": "https://...",
      "isLocked": false,
      "prerequisites": [
        {
          "lessonId": "l0",
          "required": true
        }
      ],
      "hasAssessment": false
    }
  ]
}
```

---

### 3.2 Lấy chi tiết bài học
```http
GET /lessons/{lessonId}
```

**Query Parameters:**
- `studentId` (UUID): Nếu muốn lấy progress của học sinh cụ thể

**Response (200 OK):**
```json
{
  "id": "l1",
  "title": "Bài 1: Số tự nhiên là gì",
  "description": "Giới thiệu về số tự nhiên",
  "content": "<h1>Số tự nhiên</h1><p>...</p>",
  "lessonType": "theory",
  "orderIndex": 1,
  "durationMinutes": 15,
  "videoUrl": "https://youtube.com/...",
  "videoDuration": 900,
  "isLocked": false,
  "passingScore": null,
  "prerequisites": [],
  "resources": [
    {
      "id": "r1",
      "resourceType": "pdf",
      "title": "Tài liệu bài 1",
      "url": "https://...",
      "fileSizeKb": 2048
    }
  ],
  "assessment": null,
  "comments": {
    "count": 5,
    "url": "/lessons/l1/comments"
  },
  "studentProgress": {
    "status": "in_progress",
    "completionPercentage": 60,
    "timeSpentMinutes": 12,
    "lastViewedAt": "2025-01-18T14:20:00Z"
  }
}
```

---

### 3.3 Tạo bài học
```http
POST /courses/{courseId}/chapters/{chapterId}/lessons
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Bài 1: Số tự nhiên là gì",
  "description": "...",
  "content": "<h1>Số tự nhiên</h1>...",
  "lessonType": "theory",
  "orderIndex": 1,
  "durationMinutes": 15,
  "videoUrl": "https://youtube.com/...",
  "thumbnailUrl": "https://...",
  "prerequisites": []
}
```

**Response (201 Created):**
```json
{
  "id": "l1",
  "title": "Bài 1: Số tự nhiên là gì",
  "lessonType": "theory",
  "createdAt": "2025-01-15T10:30:00Z"
}
```

---

### 3.4 Cập nhật bài học
```http
PUT /lessons/{lessonId}
Authorization: Bearer {token}
```

---

### 3.5 Xóa bài học
```http
DELETE /lessons/{lessonId}
Authorization: Bearer {token}
```

---

## 🎯 **4. ASSESSMENTS (Quiz/Bài tập)**

### 4.1 Tạo assessment
```http
POST /lessons/{lessonId}/assessments
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "title": "Quiz Chương 1",
  "description": "...",
  "assessmentType": "quiz",
  "passingScore": 70,
  "maxScore": 100,
  "timeLimitMinutes": 30,
  "attemptsAllowed": 3,
  "shuffleQuestions": true,
  "showAnswers": false,
  "questions": [
    {
      "questionText": "Số tự nhiên là gì?",
      "questionType": "multiple_choice",
      "points": 10,
      "options": [
        { "text": "Số từ 1 trở lên", "isCorrect": false },
        { "text": "Số từ 0 trở lên", "isCorrect": true },
        { "text": "Số âm", "isCorrect": false }
      ]
    },
    {
      "questionText": "Cho ví dụ về số tự nhiên",
      "questionType": "essay",
      "points": 10
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": "a1",
  "lessonId": "l1",
  "title": "Quiz Chương 1",
  "questionCount": 2,
  "createdAt": "2025-01-15T10:30:00Z"
}
```

---

### 4.2 Lấy assessment (để làm bài)
```http
GET /assessments/{assessmentId}/attempt
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "a1",
  "title": "Quiz Chương 1",
  "description": "...",
  "timeLimitMinutes": 30,
  "questions": [
    {
      "id": "q1",
      "questionText": "Số tự nhiên là gì?",
      "questionType": "multiple_choice",
      "points": 10,
      "options": [
        { "id": "opt1", "text": "Số từ 1 trở lên" },
        { "id": "opt2", "text": "Số từ 0 trở lên" },
        { "id": "opt3", "text": "Số âm" }
      ]
    }
  ],
  "attemptNumber": 1,
  "timeStartedAt": "2025-01-18T14:20:00Z"
}
```

---

### 4.3 Nộp bài làm (Submit Assessment)
```http
POST /assessments/{assessmentId}/submit
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "attemptNumber": 1,
  "answers": [
    {
      "questionId": "q1",
      "answerText": "opt2"
    },
    {
      "questionId": "q2",
      "answerText": "Số tự nhiên là những số không âm bắt đầu từ 0, 1, 2, 3..."
    }
  ],
  "timeTakenMinutes": 25
}
```

**Response (200 OK):**
```json
{
  "id": "result1",
  "studentId": "s1",
  "assessmentId": "a1",
  "attemptNumber": 1,
  "score": 18,
  "maxScore": 20,
  "percentage": 90,
  "passed": true,
  "submittedAt": "2025-01-18T14:45:00Z",
  "feedback": {
    "q1": { "correct": true, "explanation": "Đáp án chính xác!" },
    "q2": { "correct": true, "explanation": "Giải thích rất tốt" }
  }
}
```

---

### 4.4 Lấy kết quả bài làm
```http
GET /assessments/{assessmentId}/results/{resultId}
Authorization: Bearer {token}
```

---

## 👥 **5. ENROLLMENTS (Đăng ký học sinh)**

### 5.1 Lấy danh sách học sinh của khóa học
```http
GET /courses/{courseId}/enrollments
Authorization: Bearer {token}
```

**Query Parameters:**
- `status` (enum): active, completed, dropped

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "e1",
      "student": {
        "id": "s1",
        "fullName": "Nguyễn Văn B",
        "email": "student@edu.vn",
        "avatarUrl": "https://..."
      },
      "status": "active",
      "progressPercentage": 45,
      "enrolledAt": "2025-01-20T10:30:00Z",
      "totalTimeSpentMinutes": 240,
      "completedAt": null
    }
  ],
  "pagination": {
    "total": 45,
    "page": 1,
    "limit": 20
  }
}
```

---

### 5.2 Học sinh đăng ký khóa học
```http
POST /courses/{courseId}/enroll
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "studentId": "s1"  // Nếu admin đăng ký cho học sinh
  // Hoặc bỏ qua nếu là học sinh tự đăng ký (sử dụng token)
}
```

**Response (201 Created):**
```json
{
  "id": "e1",
  "courseId": "c123",
  "studentId": "s1",
  "status": "active",
  "enrolledAt": "2025-01-20T10:30:00Z"
}
```

---

### 5.3 Xóa đăng ký (Drop Course)
```http
DELETE /enrollments/{enrollmentId}
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "enrollmentId": "e1",
  "status": "dropped",
  "droppedAt": "2025-01-25T10:30:00Z"
}
```

---

## 📊 **6. PROGRESS (Tiến độ học)**

### 6.1 Lấy tiến độ học sinh trên khóa học
```http
GET /courses/{courseId}/progress
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "enrollmentId": "e1",
  "courseId": "c123",
  "progressPercentage": 45,
  "chapters": [
    {
      "id": "ch1",
      "title": "Chương 1: Số tự nhiên",
      "completionPercentage": 83,
      "lessons": [
        {
          "id": "l1",
          "title": "Bài 1: Số tự nhiên là gì",
          "status": "completed",
          "completionPercentage": 100,
          "timeSpentMinutes": 18,
          "lastViewedAt": "2025-01-18T14:20:00Z"
        }
      ]
    }
  ],
  "totalTimeSpentMinutes": 240,
  "lastActivityAt": "2025-01-18T14:45:00Z"
}
```

---

### 6.2 Cập nhật tiến độ bài học
```http
POST /lessons/{lessonId}/progress
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "status": "completed",
  "completionPercentage": 100,
  "timeSpentMinutes": 18
}
```

**Response (200 OK):**
```json
{
  "lessonId": "l1",
  "status": "completed",
  "completionPercentage": 100,
  "updatedAt": "2025-01-18T14:45:00Z"
}
```

---

## 💬 **7. COMMENTS (Bình luận)**

### 7.1 Lấy danh sách bình luận
```http
GET /lessons/{lessonId}/comments
```

**Query Parameters:**
- `page` (int)
- `limit` (int)
- `sortBy` (enum): newest, oldest, helpful

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": "c1",
      "userId": "u1",
      "user": {
        "id": "u1",
        "fullName": "Nguyễn Văn C",
        "avatarUrl": "https://..."
      },
      "content": "Bài giảng rất hay!",
      "isPinned": false,
      "likesCount": 12,
      "createdAt": "2025-01-17T10:30:00Z",
      "replies": [
        {
          "id": "c1-r1",
          "userId": "u2",
          "user": { "fullName": "Thầy A" },
          "content": "Cảm ơn em! Hãy tiếp tục cố gắng",
          "createdAt": "2025-01-17T11:30:00Z"
        }
      ]
    }
  ]
}
```

---

### 7.2 Thêm bình luận
```http
POST /lessons/{lessonId}/comments
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "content": "Bài giảng rất hay!",
  "parentCommentId": null  // null = comment gốc, khác null = reply
}
```

**Response (201 Created):**
```json
{
  "id": "c1",
  "lessonId": "l1",
  "userId": "u1",
  "content": "Bài giảng rất hay!",
  "createdAt": "2025-01-17T10:30:00Z"
}
```

---

### 7.3 Thích bình luận
```http
POST /comments/{commentId}/like
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "commentId": "c1",
  "likesCount": 13,
  "isLiked": true
}
```

---

## 🎓 **8. CERTIFICATES (Chứng chỉ)**

### 8.1 Lấy chứng chỉ
```http
GET /certificates/{certificateId}
```

**Response (200 OK):**
```json
{
  "id": "cert1",
  "studentName": "Nguyễn Văn B",
  "courseName": "Toán lớp 5",
  "issueDate": "2025-03-31",
  "certificateCode": "CERT-2025-001",
  "certificateUrl": "https://..."
}
```

---

### 8.2 Tải chứng chỉ
```http
GET /certificates/{certificateId}/download
```

**Response:** PDF file download

---

## 📈 **9. ANALYTICS (Thống kê)**

### 9.1 Lấy thống kê khóa học
```http
GET /courses/{courseId}/analytics
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "courseId": "c123",
  "totalEnrolled": 45,
  "completionRate": 60,
  "averageScore": 78.5,
  "enrollmentTrend": [
    { "date": "2025-01-20", "count": 5 },
    { "date": "2025-01-21", "count": 8 }
  ],
  "chapterEngagement": [
    {
      "chapterId": "ch1",
      "title": "Chương 1",
      "completionRate": 95,
      "averageTimeSpent": 45
    }
  ],
  "lessonDifficulty": [
    {
      "lessonId": "l3",
      "title": "Bài 3: Phép chia",
      "failureRate": 35,
      "averageScore": 65
    }
  ]
}
```

---

## 🔐 **10. ERROR RESPONSES**

### 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "Tiêu đề khóa học là bắt buộc",
  "details": [
    { "field": "title", "message": "Required" }
  ]
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Token hết hạn hoặc không hợp lệ"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "Bạn không có quyền truy cập tài nguyên này"
}
```

### 404 Not Found
```json
{
  "error": "NotFound",
  "message": "Khóa học không tồn tại"
}
```

### 409 Conflict
```json
{
  "error": "Conflict",
  "message": "Email đã được sử dụng"
}
```

### 500 Internal Server Error
```json
{
  "error": "InternalServerError",
  "message": "Đã xảy ra lỗi trên máy chủ"
}
```

---

## 🔑 **11. AUTHENTICATION**

Tất cả endpoints yêu cầu `Authorization: Bearer {token}` (trừ những endpoint công khai như GET /courses)

**Bearer Token Format:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

Token JWT chứa:
```json
{
  "sub": "user-id",
  "email": "user@edu.vn",
  "role": "student|instructor|admin",
  "iat": 1234567890,
  "exp": 1234571490
}
```

---

## 📑 **12. PAGINATION**

Tất cả list endpoints hỗ trợ pagination:

**Query Parameters:**
- `page` (int, default=1): Trang hiện tại
- `limit` (int, default=10): Số lượng items trên trang

**Response Structure:**
```json
{
  "data": [...],
  "pagination": {
    "total": 120,
    "page": 1,
    "limit": 10,
    "totalPages": 12,
    "hasNextPage": true,
    "hasPrevPage": false
  }
}
```

