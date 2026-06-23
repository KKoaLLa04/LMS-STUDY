# 📊 Database Schema - Hệ thống Quản lý Khóa học

## 1. Entities Chính & Attributes

### 1.1 **Courses** (Khóa học)
```sql
CREATE TABLE courses (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  title VARCHAR(255) NOT NULL,           -- Tiêu đề khóa học
  description TEXT,                      -- Mô tả chi tiết
  category_id UUID NOT NULL,             -- Danh mục (Toán, Văn, ...)
  instructor_id UUID NOT NULL,           -- Giáo viên tạo khóa học
  level ENUM('elementary', 'middle_school') NOT NULL, -- Cấp độ
  thumbnail_url VARCHAR(500),            -- Hình đại diện
  
  -- Quản lý trạng thái
  status ENUM('draft', 'under_review', 'published', 'archived') DEFAULT 'draft',
  version INT DEFAULT 1,                 -- Phiên bản (v1, v2...)
  
  -- Quy tắc tham gia
  max_students INT,                      -- Giới hạn học sinh (null = unlimited)
  enrollment_open BOOLEAN DEFAULT true,
  start_date TIMESTAMP,
  end_date TIMESTAMP,
  
  -- Metadata
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  published_at TIMESTAMP,
  
  FOREIGN KEY (instructor_id) REFERENCES users(id),
  FOREIGN KEY (category_id) REFERENCES course_categories(id)
);

CREATE INDEX idx_courses_instructor ON courses(instructor_id);
CREATE INDEX idx_courses_status ON courses(status);
```

---

### 1.2 **Chapters** (Chương)
```sql
CREATE TABLE chapters (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  course_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  order_index INT NOT NULL,              -- Thứ tự chương trong khóa học
  
  -- Điều kiện hoàn thành
  min_completion_percentage INT DEFAULT 80, -- % bài học tối thiểu để hoàn thành chương
  pass_required BOOLEAN DEFAULT false,
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
  UNIQUE(course_id, order_index)
);

CREATE INDEX idx_chapters_course ON chapters(course_id);
```

---

### 1.3 **Lessons** (Bài học)
```sql
CREATE TABLE lessons (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  chapter_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  content TEXT,                          -- Nội dung HTML/Markdown
  lesson_type ENUM('theory', 'practice', 'quiz', 'project') NOT NULL,
  order_index INT NOT NULL,              -- Thứ tự bài học trong chương
  
  -- Điều kiện & yêu cầu
  duration_minutes INT,                  -- Thời gian dự kiến (phút)
  is_locked BOOLEAN DEFAULT false,       -- Bài học bị khóa?
  prerequisites JSONB,                   -- [{lessonId, required: true/false}] - Bài học tiên quyết
  
  -- Điểm số (nếu có assessment)
  passing_score INT,                     -- Điểm tối thiểu để pass (0-100)
  max_score INT DEFAULT 100,
  
  -- Media & tài nguyên
  video_url VARCHAR(500),
  video_duration_seconds INT,
  thumbnail_url VARCHAR(500),
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (chapter_id) REFERENCES chapters(id) ON DELETE CASCADE,
  UNIQUE(chapter_id, order_index)
);

CREATE INDEX idx_lessons_chapter ON lessons(chapter_id);
CREATE INDEX idx_lessons_type ON lessons(lesson_type);
```

---

### 1.4 **Lesson Resources** (Tài nguyên bài học)
```sql
CREATE TABLE lesson_resources (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  lesson_id UUID NOT NULL,
  resource_type ENUM('pdf', 'image', 'document', 'code', 'link') NOT NULL,
  title VARCHAR(255),
  url VARCHAR(500) NOT NULL,
  file_size_kb INT,
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (lesson_id) REFERENCES lessons(id) ON DELETE CASCADE
);

CREATE INDEX idx_resources_lesson ON lesson_resources(lesson_id);
```

---

### 1.5 **Assessments** (Đánh giá - Quiz/Bài tập)
```sql
CREATE TABLE assessments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  lesson_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  description TEXT,
  assessment_type ENUM('quiz', 'assignment') NOT NULL,
  passing_score INT DEFAULT 70,
  max_score INT DEFAULT 100,
  time_limit_minutes INT,                 -- Giới hạn thời gian (phút)
  shuffle_questions BOOLEAN DEFAULT false,
  
  -- Cấu hình
  attempts_allowed INT DEFAULT 3,         -- Số lần làm bài tối đa
  show_answers BOOLEAN DEFAULT false,     -- Hiển thị đáp án sau khi hoàn thành
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (lesson_id) REFERENCES lessons(id) ON DELETE CASCADE,
  UNIQUE(lesson_id)                       -- 1 assessment trên 1 lesson
);

CREATE INDEX idx_assessments_lesson ON assessments(lesson_id);
```

---

### 1.6 **Assessment Questions** (Câu hỏi)
```sql
CREATE TABLE assessment_questions (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  assessment_id UUID NOT NULL,
  question_text TEXT NOT NULL,
  question_type ENUM('multiple_choice', 'essay', 'matching', 'fill_blank') NOT NULL,
  order_index INT NOT NULL,
  points INT DEFAULT 1,                  -- Điểm cho câu hỏi này
  
  -- Dữ liệu tuỳ loại
  options JSONB,                         -- [{id, text, isCorrect}] cho multiple_choice
  correct_answer VARCHAR(500),           -- Đáp án chính xác (essay/fill_blank)
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (assessment_id) REFERENCES assessments(id) ON DELETE CASCADE,
  UNIQUE(assessment_id, order_index)
);

CREATE INDEX idx_questions_assessment ON assessment_questions(assessment_id);
```

---

### 1.7 **Student Enrollment** (Đăng ký học sinh)
```sql
CREATE TABLE student_enrollments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id UUID NOT NULL,
  course_id UUID NOT NULL,
  
  status ENUM('active', 'suspended', 'completed', 'dropped') DEFAULT 'active',
  progress_percentage INT DEFAULT 0,     -- % khóa học hoàn thành
  
  enrolled_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  started_at TIMESTAMP,                  -- Lần đầu tiên học
  completed_at TIMESTAMP,                -- Ngày hoàn thành khóa học
  
  total_time_spent_minutes INT DEFAULT 0, -- Tổng thời gian học (phút)
  
  FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
  UNIQUE(student_id, course_id)
);

CREATE INDEX idx_enrollments_student ON student_enrollments(student_id);
CREATE INDEX idx_enrollments_course ON student_enrollments(course_id);
CREATE INDEX idx_enrollments_status ON student_enrollments(status);
```

---

### 1.8 **Lesson Progress** (Tiến độ bài học)
```sql
CREATE TABLE lesson_progress (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id UUID NOT NULL,
  lesson_id UUID NOT NULL,
  
  status ENUM('not_started', 'in_progress', 'completed', 'passed', 'failed') DEFAULT 'not_started',
  completion_percentage INT DEFAULT 0,
  last_viewed_at TIMESTAMP,
  started_at TIMESTAMP,
  completed_at TIMESTAMP,
  
  time_spent_minutes INT DEFAULT 0,
  
  FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (lesson_id) REFERENCES lessons(id) ON DELETE CASCADE,
  UNIQUE(student_id, lesson_id)
);

CREATE INDEX idx_lesson_progress_student ON lesson_progress(student_id);
CREATE INDEX idx_lesson_progress_lesson ON lesson_progress(lesson_id);
```

---

### 1.9 **Student Answers** (Câu trả lời của học sinh)
```sql
CREATE TABLE student_answers (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id UUID NOT NULL,
  assessment_id UUID NOT NULL,
  question_id UUID NOT NULL,
  
  attempt_number INT NOT NULL,           -- Lần thứ mấy làm bài
  answer_text TEXT,                      -- Câu trả lời học sinh
  is_correct BOOLEAN,
  points_earned INT,
  
  answered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (assessment_id) REFERENCES assessments(id) ON DELETE CASCADE,
  FOREIGN KEY (question_id) REFERENCES assessment_questions(id) ON DELETE CASCADE
);

CREATE INDEX idx_answers_student_assessment ON student_answers(student_id, assessment_id, attempt_number);
```

---

### 1.10 **Assessment Results** (Kết quả bài kiểm tra)
```sql
CREATE TABLE assessment_results (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id UUID NOT NULL,
  assessment_id UUID NOT NULL,
  
  attempt_number INT NOT NULL,
  score INT,                             -- Điểm tổng cộng
  max_score INT,
  percentage INT,                        -- % (0-100)
  passed BOOLEAN,
  time_taken_minutes INT,
  
  submitted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (assessment_id) REFERENCES assessments(id) ON DELETE CASCADE,
  UNIQUE(student_id, assessment_id, attempt_number)
);

CREATE INDEX idx_results_student ON assessment_results(student_id);
CREATE INDEX idx_results_assessment ON assessment_results(assessment_id);
```

---

### 1.11 **Comments** (Bình luận trên bài học)
```sql
CREATE TABLE lesson_comments (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  lesson_id UUID NOT NULL,
  user_id UUID NOT NULL,
  content TEXT NOT NULL,
  
  parent_comment_id UUID,                -- null = comment gốc, khác null = reply
  likes_count INT DEFAULT 0,
  
  is_pinned BOOLEAN DEFAULT false,       -- Bình luận của giáo viên được ghim
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  FOREIGN KEY (lesson_id) REFERENCES lessons(id) ON DELETE CASCADE,
  FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (parent_comment_id) REFERENCES lesson_comments(id) ON DELETE CASCADE
);

CREATE INDEX idx_comments_lesson ON lesson_comments(lesson_id);
CREATE INDEX idx_comments_user ON lesson_comments(user_id);
```

---

### 1.12 **Certificates** (Chứng chỉ hoàn thành)
```sql
CREATE TABLE certificates (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  student_id UUID NOT NULL,
  course_id UUID NOT NULL,
  
  issue_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  certificate_url VARCHAR(500),          -- URL tải chứng chỉ
  certificate_code VARCHAR(50) UNIQUE,   -- Mã xác thực duy nhất
  
  FOREIGN KEY (student_id) REFERENCES users(id) ON DELETE CASCADE,
  FOREIGN KEY (course_id) REFERENCES courses(id) ON DELETE CASCADE,
  UNIQUE(student_id, course_id)          -- 1 chứng chỉ trên 1 khóa/1 học sinh
);

CREATE INDEX idx_certificates_student ON certificates(student_id);
```

---

### 1.13 **Users** (Người dùng)
```sql
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  email VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(500),
  full_name VARCHAR(255) NOT NULL,
  role ENUM('student', 'instructor', 'admin') NOT NULL,
  
  avatar_url VARCHAR(500),
  bio TEXT,
  
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  
  INDEX idx_users_email (email)
);
```

---

### 1.14 **Course Categories** (Danh mục khóa học)
```sql
CREATE TABLE course_categories (
  id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name VARCHAR(100) NOT NULL UNIQUE,
  description TEXT,
  icon_url VARCHAR(500),
  
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 2. Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────────────────────────────────────┐
│                          USERS (Người dùng)                         │
├─────────────────────────────────────────────────────────────────────┤
│ id (PK), email, full_name, role, avatar_url, created_at            │
└──────────────┬──────────────────────────────┬──────────────────────┘
               │                              │
        (instructor)                    (student)
               │                              │
               ▼                              ▼
    ┌─────────────────────┐      ┌──────────────────────┐
    │ COURSES (Khóa học)  │      │ STUDENT_ENROLLMENTS  │
    ├─────────────────────┤      │   (Đăng ký học sinh) │
    │ id (PK)             │◄─────┤ id (PK)              │
    │ title               │      │ student_id (FK)      │
    │ instructor_id (FK)  │      │ course_id (FK)       │
    │ category_id (FK)    │      │ status               │
    │ status              │      │ progress_percentage  │
    │ version             │      │ enrolled_at          │
    └────────────┬────────┘      └──────────────────────┘
                 │
                 ▼
        ┌─────────────────┐
        │ CHAPTERS        │
        │ (Chương học)    │
        ├─────────────────┤
        │ id (PK)         │
        │ course_id (FK)  │
        │ title           │
        │ order_index     │
        └────────┬────────┘
                 │
                 ▼
        ┌─────────────────────┐
        │ LESSONS             │
        │ (Bài học)           │
        ├─────────────────────┤
        │ id (PK)             │
        │ chapter_id (FK)     │
        │ title               │
        │ lesson_type         │
        │ order_index         │
        │ prerequisites       │
        └────────┬────────────┘
                 │
          ┌──────┴──────┐
          │             │
          ▼             ▼
    ┌──────────┐  ┌──────────────┐
    │RESOURCES │  │ASSESSMENTS   │
    │(Tài nguy)│  │(Quiz/Bài tập)│
    └──────────┘  └──────┬───────┘
                         │
                         ▼
                 ┌──────────────────┐
                 │ASSESSMENT_QUESTIONS│
                 │(Câu hỏi)         │
                 └──────────────────┘

    ┌──────────────────┐
    │ LESSON_PROGRESS  │
    │ (Tiến độ học)    │
    ├──────────────────┤
    │ student_id (FK)  │
    │ lesson_id (FK)   │
    │ status           │
    │ completion_%     │
    └──────────────────┘

    ┌──────────────────┐
    │ASSESSMENT_RESULTS│
    │(Kết quả bài)     │
    ├──────────────────┤
    │ student_id (FK)  │
    │ assessment_id(FK)│
    │ score            │
    │ passed           │
    └──────────────────┘

    ┌────────────────┐
    │ CERTIFICATES   │
    │ (Chứng chỉ)    │
    ├────────────────┤
    │ student_id(FK) │
    │ course_id (FK) │
    │ issued_date    │
    └────────────────┘
```

---

## 3. Keys & Relationships

| Parent Table | Child Table | Relationship | Cascade |
|---|---|---|---|
| courses | chapters | 1:N | ON DELETE CASCADE |
| chapters | lessons | 1:N | ON DELETE CASCADE |
| lessons | lesson_resources | 1:N | ON DELETE CASCADE |
| lessons | assessments | 1:1 | ON DELETE CASCADE |
| assessments | assessment_questions | 1:N | ON DELETE CASCADE |
| users | courses | 1:N (instructor) | SET NULL |
| users | student_enrollments | 1:N | ON DELETE CASCADE |
| courses | student_enrollments | 1:N | ON DELETE CASCADE |
| users | lesson_progress | 1:N | ON DELETE CASCADE |
| lessons | lesson_progress | 1:N | ON DELETE CASCADE |
| users | student_answers | 1:N | ON DELETE CASCADE |
| assessments | student_answers | 1:N | ON DELETE CASCADE |
| assessment_questions | student_answers | 1:N | ON DELETE CASCADE |
| assessments | assessment_results | 1:N | ON DELETE CASCADE |
| users | certificates | 1:N | ON DELETE CASCADE |
| courses | certificates | 1:N | ON DELETE CASCADE |

---

## 4. Constraints & Business Rules

✅ **Unique Constraints**
- `user.email` - Email duy nhất
- `courses.id + version` - Phiên bản khóa học
- `student_enrollments (student_id, course_id)` - Học sinh chỉ đăng ký 1 lần
- `lesson_progress (student_id, lesson_id)` - Mỗi bài học 1 lần theo dõi
- `assessment_results (student_id, assessment_id, attempt_number)` - Kết quả của mỗi lần làm bài

✅ **Check Constraints** (nên thêm)
```sql
-- Điểm không được vượt quá max_score
ALTER TABLE assessments ADD CONSTRAINT check_score 
  CHECK (passing_score <= max_score);

-- Progress % từ 0-100
ALTER TABLE lesson_progress ADD CONSTRAINT check_completion 
  CHECK (completion_percentage BETWEEN 0 AND 100);

-- order_index phải > 0
ALTER TABLE chapters ADD CONSTRAINT check_order 
  CHECK (order_index > 0);
```

✅ **Business Rules**
- Học sinh chỉ được xem bài học **Published** (status = 'published')
- Bài học bị khóa nếu chưa hoàn thành bài học tiên quyết
- Chứng chỉ chỉ được cấp khi `enrollment.status = 'completed'` và hoàn thành tất cả chapters
- Học sinh chỉ có `attempts_allowed` lần để làm assessment

