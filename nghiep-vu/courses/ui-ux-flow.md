# 🎨 UI/UX Flow & Screen Design - Hệ thống Quản lý Khóa học

---

## 📋 **1. User Journey & Flow Diagrams**

### 1.1 STUDENT (Học sinh) - User Journey

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        STUDENT JOURNEY MAP                              │
└─────────────────────────────────────────────────────────────────────────┘

1. DISCOVERY & ENROLLMENT
   ┌──────────────┐
   │ Browse       │──→ Search/Filter  ──→ View Course ──→ Enroll
   │ Courses      │    (Category,       Details       (Join)
   │              │     Level)
   └──────────────┘

2. LEARNING
   ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
   │ View Chapter │ ──→ │ Watch Lesson  │ ──→ │ Read Content │
   │ & Lessons    │     │ (Theory/Video)│     │ & Resources  │
   └──────────────┘     └──────────────┘     └──────────────┘
                              │
                              ▼
                        ┌──────────────┐
                        │ Q&A Comments │
                        │ Ask/Reply    │
                        └──────────────┘

3. ASSESSMENT
   ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
   │ Start Quiz   │ ──→ │ Answer Qs    │ ──→ │ Submit & View│
   │ (Timer)      │     │ (Multiple    │     │ Score        │
   └──────────────┘     │  choice/    │     └──────────────┘
                        │  essay)      │
                        └──────────────┘
                             │
                             ▼
                        ┌──────────────┐
                        │ Review Answer│
                        │ & Feedback   │
                        └──────────────┘

4. COMPLETION & CERTIFICATION
   ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
   │ Complete     │ ──→ │ Course 100%  │ ──→ │ Get Cert     │
   │ All Chapters │     │ Completion   │     │ (Download)   │
   └──────────────┘     └──────────────┘     └──────────────┘
```

---

### 1.2 INSTRUCTOR (Giáo viên) - Course Creation Flow

```
┌────────────────────────────────────────────────────────────┐
│               INSTRUCTOR COURSE CREATION FLOW               │
└────────────────────────────────────────────────────────────┘

1. CREATE COURSE
   ┌─────────────────┐
   │ Create New      │ ──→ Basic Info ──→ Category, Level, 
   │ Course          │     (Title, Desc)   Duration
   └─────────────────┘

2. BUILD STRUCTURE
   ┌──────────────────┐     ┌──────────────┐     ┌──────────────┐
   │ Add Chapters     │ ──→ │ Add Lessons  │ ──→ │ Add Content  │
   │ (Order, Min%)    │     │ (Type, Order)│     │ (Video, PDF) │
   └──────────────────┘     └──────────────┘     └──────────────┘

3. CREATE ASSESSMENTS
   ┌──────────────────┐
   │ Add Quiz/Tests   │ ──→ Add Questions (MC/Essay/Matching)
   │ (Per Lesson)     │     Set Passing Score
   └──────────────────┘

4. SAVE DRAFT
   ┌──────────────────┐
   │ Save as Draft    │ ──→ Can edit anytime
   └──────────────────┘

5. REVIEW & PUBLISH
   ┌──────────────────┐     ┌──────────────┐     ┌──────────────┐
   │ Review Course    │ ──→ │ Check All    │ ──→ │ Publish      │
   │ Completeness     │     │ Content OK   │     │ (Go Live)    │
   └──────────────────┘     └──────────────┘     └──────────────┘

6. MANAGE & ANALYZE
   ┌──────────────────┐     ┌──────────────┐     ┌──────────────┐
   │ View Students    │ ──→ │ Check        │ ──→ │ Get Feedback │
   │ Progress         │     │ Engagement   │     │ (Comments)   │
   └──────────────────┘     └──────────────┘     └──────────────┘
```

---

### 1.3 ADMIN (Quản trị viên) - Management Flow

```
┌────────────────────────────────────────────────┐
│           ADMIN MANAGEMENT FLOW                │
└────────────────────────────────────────────────┘

1. COURSE MODERATION
   ┌──────────────┐
   │ Review       │ ──→ Under Review ──→ ✅ Approve
   │ Submissions  │                  \─→ ❌ Reject
   └──────────────┘

2. USER MANAGEMENT
   ┌──────────────┐
   │ Manage Users │ ──→ View | Edit | Deactivate
   │ (S, I, A)    │
   └──────────────┘

3. CATEGORY MANAGEMENT
   ┌──────────────┐
   │ Manage       │ ──→ Create | Update | Delete
   │ Categories   │
   └──────────────┘

4. ANALYTICS & REPORTS
   ┌──────────────┐
   │ System       │ ──→ User Growth | Course Stats | Engagement
   │ Analytics    │
   └──────────────┘
```

---

## 🖥️ **2. Screen Designs & Wireframes**

### 2.1 **HOMEPAGE / COURSE LISTING (Public)**

```
┌─────────────────────────────────────────────────────────────┐
│  Logo  │ Courses | My Learning | Sign In │ Instructor Mode  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  🔍 Search Courses...  [Filter] [Sort ▼]                   │
│                                                              │
│  Category: [All] [Math] [Science] [Language] [Art]          │
│  Level: [All] [Elementary] [Middle School]                  │
│                                                              │
├─────────────────────────────────────────────────────────────┤
│                    FEATURED COURSES                          │
├─────────────────────────────────────────────────────────────┤
│
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  │              │  │              │  │              │
│  │ [Thumbnail]  │  │ [Thumbnail]  │  │ [Thumbnail]  │
│  │              │  │              │  │              │
│  │ Toán Lớp 5   │  │ Văn Lớp 6    │  │ Tiếng Anh 7  │
│  │              │  │              │  │              │
│  │ 32 lessons   │  │ 28 lessons   │  │ 45 lessons   │
│  │ ⭐ 4.5 (120) │  │ ⭐ 4.2 (85)  │  │ ⭐ 4.8 (200) │
│  │              │  │              │  │              │
│  │ 45/50        │  │ 38/40        │  │ 50/50 Full   │
│  │ Students     │  │ Students     │  │              │
│  │              │  │              │  │              │
│  │   [Enroll]   │  │   [Enroll]   │  │  [View Info] │
│  └──────────────┘  └──────────────┘  └──────────────┘
│
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│  │...More...    │  │              │  │              │
│  └──────────────┘  └──────────────┘  └──────────────┘
│
└─────────────────────────────────────────────────────────────┘
```

---

### 2.2 **COURSE DETAIL PAGE**

```
┌─────────────────────────────────────────────────────────────┐
│ ← Back | Course Dashboard                                   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌─────────────────────┐  ┌──────────────────────────────┐ │
│  │                     │  │  Toán Lớp 5                  │ │
│  │    [Thumbnail]      │  │  (Elementary)                │ │
│  │   Video Icon        │  │                              │ │
│  │   Duration: 15:30   │  │  ⭐ 4.5 (120 reviews)        │ │
│  │                     │  │                              │ │
│  │                     │  │  By: Nguyễn Văn A            │ │
│  │                     │  │  Published: Jan 20, 2025     │ │
│  │                     │  │                              │ │
│  └─────────────────────┘  │  📊 45/50 Students Enrolled  │ │
│                           │  📅 Starts: Jan 20, 2025     │ │
│                           │  📅 Ends: Mar 31, 2025       │ │
│                           │                              │ │
│                           │  ┌────────────────────────┐ │ │
│                           │  │ [Enroll Now] [Share]   │ │ │
│                           │  └────────────────────────┘ │ │
│                           └──────────────────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ COURSE OVERVIEW                                         │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ Khóa học Toán cơ bản cho học sinh tiểu học lớp 5       │ │
│ │ Bao gồm 5 chương với 32 bài học chi tiết, bài tập      │ │
│ │ và kiểm tra thường xuyên...                            │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ CHAPTERS (5 chapters) | REVIEWS                         │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ ✓ Chapter 1: Số tự nhiên           (6 lessons)        │ │
│ │   └─ Bài 1: Số tự nhiên là gì (15 min)  [Video]       │ │
│ │   └─ Bài 2: So sánh số tự nhiên (20 min) [Theory]     │ │
│ │   └─ Bài 3: Phép cộng (18 min)          [Theory+Quiz] │ │
│ │                                                         │ │
│ │ ○ Chapter 2: Phép tính cơ bản       (7 lessons)        │ │
│ │   └─ Bài 1: Phép cộng... (Locked)                      │ │
│ │                                                         │ │
│ │ ○ Chapter 3: ...                                        │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.3 **LESSON PLAYER / CONTENT VIEW**

```
┌─────────────────────────────────────────────────────────────┐
│ ← Back | Toán Lớp 5 > Chương 1 > Bài 1                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                                                         │ │
│ │               [Video Player]                           │ │
│ │               (Fullscreen)                             │ │
│ │                                                         │ │
│ │         ▶️  ───●────────────── (15:30 / 15:30)        │ │
│ │         Vol: 🔊     Settings ⚙️  Fullscreen ⛶        │ │
│ │                                                         │ │
│ │ [Auto-marked as watched when completed]               │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  📝 Content & Resources                                     │ │
│  ─────────────────────────────────────────────────────────  │ │
│  ## Số tự nhiên là gì?                                      │ │
│  Số tự nhiên là những số...                                 │ │
│                                                              │ │
│  📎 Resources:                                              │ │
│  • [📄 Tài liệu Bài 1.pdf] (2MB)                           │ │
│  • [🖼️ Hình minh họa.png] (500KB)                          │ │
│                                                              │ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ COMMENTS (5 comments)                                  │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │ 👤 Nguyễn Văn C  2 days ago                           │ │
│ │    Bài giảng rất hay! ❤️ 12  Reply                     │ │
│ │                                                         │ │
│ │    └─👤 Thầy A  1 day ago                             │ │
│ │      Cảm ơn em! Hãy tiếp tục cố gắng                  │ │
│ │                                                         │ │
│ │ 👤 Trần Thị D  3 days ago                             │ │
│ │    Có thể giải thích lại điểm này không? Reply        │ │
│ │                                                         │ │
│ │ ┌──────────────────────────────────────────┐           │ │
│ │ │ 💬 Add comment...                         │ [Post]   │ │
│ │ └──────────────────────────────────────────┘           │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│  │ ⬅️ Previous  │    │ Mark as Done │    │ Next ➡️      │  │
│  │   Lesson     │    │              │    │  Lesson      │  │
│  └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.4 **QUIZ / ASSESSMENT PLAYER**

```
┌─────────────────────────────────────────────────────────────┐
│ Quiz: Kiểm tra Chương 1                      ⏱️ 25:30 / 30:00
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Question 1 of 5                                            │
│  ═════════════════════════════════════════════════════════ │
│                                                              │
│  Số tự nhiên là gì?                                         │
│                                                              │
│  ○ Số từ 1 trở lên                                          │
│  ⦿ Số từ 0 trở lên                                          │
│  ○ Số âm                                                    │
│  ○ Tất cả những số                                          │
│                                                              │
│  ──────────────────────────────────────────────────────────  │
│                                                              │
│  Question 2 of 5                                            │
│  ═════════════════════════════════════════════════════════ │
│                                                              │
│  Cho ví dụ về số tự nhiên:                                  │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ Số tự nhiên bao gồm 0, 1, 2, 3, 4...               │  │
│  │                                                     │  │
│  └─────────────────────────────────────────────────────┘  │
│                                                              │
│  ──────────────────────────────────────────────────────────  │
│                                                              │
│ Question Progress: ●●●○○ (3/5 answered)                    │
│                                                              │
│ ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│ │ ⬅️ Back      │    │ Review All   │    │ Submit ➡️    │  │
│ │              │    │              │    │              │  │
│ └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                              │
│ ⚠️ Confirm submission? You have 2 unanswered questions.    │
│                                                              │
│    [Cancel] [Submit Anyway]                                │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.5 **QUIZ RESULT PAGE**

```
┌─────────────────────────────────────────────────────────────┐
│ Quiz Result: Kiểm tra Chương 1                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│                    🎉 Đạt! 🎉                               │
│                                                              │
│                    Score: 18/20                             │
│                    Percentage: 90%                          │
│                    Status: ✅ Passed                        │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ DETAILED FEEDBACK                                       │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │ Q1: Số tự nhiên là gì?                                │ │
│ │ Your answer: ⦿ Số từ 0 trở lên                        │ │
│ │ Result: ✅ Correct (+10 points)                        │ │
│ │                                                         │ │
│ │ Q2: Cho ví dụ về số tự nhiên:                          │ │
│ │ Your answer: Số tự nhiên bao gồm 0, 1, 2, 3, 4...     │ │
│ │ Result: ✅ Correct (+10 points)                        │ │
│ │ Explanation: Giải thích rất tốt!                       │ │
│ │                                                         │ │
│ │ Q3: ...                                                │ │
│ │ Result: ❌ Incorrect (0 points)                        │ │
│ │ Correct answer: ... (bạn trả lời: ...)                │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ Attempt 1 of 3  (2 attempts remaining)                      │
│                                                              │
│ ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│ │ ⬅️ Back      │    │ Retry        │    │ Next Lesson →│  │
│ │              │    │              │    │              │  │
│ └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.6 **STUDENT PROGRESS / DASHBOARD**

```
┌─────────────────────────────────────────────────────────────┐
│ Home | My Learning | My Certificates | Settings            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  👤 Xin chào, Nguyễn Văn B!                                │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ MY LEARNING - ACTIVE COURSES                            │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │  1. Toán Lớp 5                                          │ │
│ │     ████████░░ 80% completed                           │ │
│ │                                                         │ │
│ │     Progress:                                           │ │
│ │     • Chapter 1: ✅ 100%                               │ │
│ │     • Chapter 2: ✅ 100%                               │ │
│ │     • Chapter 3: 🔄 60%  (3/5 lessons)                │ │
│ │     • Chapter 4: ○ 0%    (Locked)                     │ │
│ │     • Chapter 5: ○ 0%    (Locked)                     │ │
│ │                                                         │ │
│ │     Last activity: 2 hours ago                          │ │
│ │     Time spent: 12 hours 30 minutes                     │ │
│ │                                                         │ │
│ │     [Continue Learning] [View Details]                 │ │
│ │                                                         │ │
│ │  ─────────────────────────────────────────────────────  │ │
│ │                                                         │ │
│ │  2. Văn Lớp 6                                           │ │
│ │     ███░░░░░░░ 30% completed                           │ │
│ │                                                         │ │
│ │     Last activity: 1 week ago                           │ │
│ │     [Continue Learning]                                │ │
│ │                                                         │ │
│ │  ─────────────────────────────────────────────────────  │ │
│ │                                                         │ │
│ │  ┌─────────────────────────────────────────┐            │ │
│ │  │ View All Courses (3 enrolled)            │ [→]      │ │
│ │  └─────────────────────────────────────────┘            │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ COMPLETED COURSES & CERTIFICATES                        │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │  🎓 Tiếng Anh 7  (Completed Jan 25, 2025)             │ │
│ │     [Download Certificate] [Share]                     │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.7 **INSTRUCTOR: CREATE COURSE**

```
┌─────────────────────────────────────────────────────────────┐
│ Instructor | My Courses | New Course | Instructor Settings │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ CREATE NEW COURSE                                            │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ BASIC INFORMATION                                       │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │ Title: ___________________________________              │ │
│ │ (e.g., "Toán lớp 5")                                   │ │
│ │                                                         │ │
│ │ Description:                                            │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │ [Rich Text Editor]                                  │ │ │
│ │ │                                                     │ │ │
│ │ │ Khóa học Toán cơ bản cho học sinh tiểu học lớp 5   │ │ │
│ │ │                                                     │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ │                                                         │ │
│ │ Category: [🔽 Select Category]  Level: [🔽 Elementary] │ │
│ │                                                         │ │
│ │ Thumbnail: [Upload Image] [Choose from Library]         │ │
│ │ ┌────────────┐                                          │ │
│ │ │ [Thumbnail]│                                          │ │
│ │ │ Preview    │                                          │ │
│ │ └────────────┘                                          │ │
│ │                                                         │ │
│ │ Max Students: 50    (Unlimited: ☐)                     │ │
│ │ Duration: 2 months                                      │ │
│ │ Start Date: [2025-01-20]  End Date: [2025-03-31]      │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ COURSE STRUCTURE                                        │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │ Chapters (5 chapters)                                   │ │
│ │ ┌─────────────────────────────────────────────────────┐ │ │
│ │ │                                                     │ │ │
│ │ │ ✓ Chapter 1: Số tự nhiên                           │ │ │
│ │ │   └─ 6 Lessons  | ✏️  | 🗑️                         │ │ │
│ │ │                                                     │ │ │
│ │ │ ✓ Chapter 2: Phép tính cơ bản                       │ │ │
│ │ │   └─ 7 Lessons  | ✏️  | 🗑️                         │ │ │
│ │ │                                                     │ │ │
│ │ │ ✓ Chapter 3: Phân số                               │ │ │
│ │ │   └─ 6 Lessons  | ✏️  | 🗑️                         │ │ │
│ │ │                                                     │ │ │
│ │ │ [+ Add Chapter]                                     │ │ │
│ │ │                                                     │ │ │
│ │ └─────────────────────────────────────────────────────┘ │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ ┌──────────────┐    ┌──────────────┐    ┌──────────────┐  │
│ │ Save Draft   │    │ Preview      │    │ Publish      │  │
│ │              │    │              │    │              │  │
│ └──────────────┘    └──────────────┘    └──────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 2.8 **INSTRUCTOR: MANAGE STUDENTS & ANALYTICS**

```
┌─────────────────────────────────────────────────────────────┐
│ Course: Toán Lớp 5 | Overview | Students | Analytics       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│ STUDENT MANAGEMENT                                           │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Filter: [Status: All ▼] [Search: ________] [Export CSV] │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                         │ │
│ │ # | Student Name      | Email           | Progress | S │ │
│ │   │                   |                 |          | t │ │
│ │───┼───────────────────┼─────────────────┼──────────┼──│ │
│ │ 1 │ Nguyễn Văn B      │ b@school.edu.vn │ 80% ✅  │ A │ │
│ │ 2 │ Trần Thị C        │ c@school.edu.vn │ 45% 🔄  │ A │ │
│ │ 3 │ Phạm Văn D        │ d@school.edu.vn │ 20% 🔄  │ A │ │
│ │ 4 │ Lê Thị E          │ e@school.edu.vn │ 100%✅  │ C │ │
│ │ 5 │ Hoàng Văn F       │ f@school.edu.vn │ 30% 🔄  │ A │ │
│ │ ...                                                    │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ COURSE ANALYTICS                                             │
│                                                              │
│ ┌────────────────┐  ┌────────────────┐  ┌────────────────┐ │
│ │ Total Students │  │ Avg Completion │  │ Avg Score      │ │
│ │      45        │  │      62%       │  │    76.5/100    │ │
│ └────────────────┘  └────────────────┘  └────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ ENROLLMENT TREND (Last 30 Days)                         │ │
│ │                                                         │ │
│ │   📊 Chart                                              │ │
│ │                                                         │ │
│ │   Week 1: 10 | Week 2: 15 | Week 3: 12 | Week 4: 8    │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ LESSON DIFFICULTY REPORT                                │ │
│ │                                                         │ │
│ │ Hardest Lessons:                                        │ │
│ │ 1. Bài 3: Phép chia        - 35% failure rate  ⚠️      │ │
│ │ 2. Bài 7: Phân số cơ bản   - 28% failure rate  ⚠️      │ │
│ │ 3. Bài 5: Phép nhân        - 20% failure rate           │ │
│ │                                                         │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔄 **3. State Transitions & Workflows**

### 3.1 **Course Status Workflow**

```
┌─────────────────────────────────────────────────────────────┐
│              COURSE LIFECYCLE / STATUS FLOW                  │
└─────────────────────────────────────────────────────────────┘

  CREATE                    SAVE                   SUBMIT
   ↓                         ↓                       ↓
┌────────┐  (editing)  ┌──────────┐  (ready)  ┌────────────┐
│ DRAFT  │────────────→│  DRAFT   │─────────→│UNDER_REVIEW│
└────────┘             └──────────┘          └────────────┘
                                                    │
                                              APPROVE│ REJECT
                                                    ↓  ↓
  ARCHIVE ←─────────────────────────────┐  ┌──────────┐
    ↓                                    │  │PUBLISHED │
  ┌───────┐                             │  └──────────┘
  │ARCHIVE│◄─────────────────────────────┘
  └───────┘       (instructor/admin action)

  Key:
  • Student can only view PUBLISHED courses
  • Instructor can edit DRAFT or Rejected courses
  • Admin reviews UNDER_REVIEW courses
  • Once published, new version = new submission needed
```

---

### 3.2 **Enrollment Status Flow**

```
┌─────────────────────────────────────────────────────────────┐
│           STUDENT ENROLLMENT LIFECYCLE                       │
└─────────────────────────────────────────────────────────────┘

  ENROLL
    ↓
┌──────────┐  (learning)  ┌────────────┐  (finished)  ┌──────────┐
│  ACTIVE  │─────────────→│  IN_PROG   │────────────→│COMPLETED │
└──────────┘              └────────────┘              └──────────┘
    ↑                           │
    │                      DROP │
    │                           ↓
    └──────────────────┬─  ┌───────────┐
                       └→│  DROPPED   │
                         └───────────┘

  Key:
  • ACTIVE: Just enrolled, hasn't started
  • IN_PROGRESS: Started learning
  • COMPLETED: Finished all chapters + passed min score
  • DROPPED: Student withdrew from course
  • SUSPENDED: Admin suspended (optional)

  Completion = 100% chapters + pass required assessments
```

---

### 3.3 **Lesson Progress Flow**

```
┌─────────────────────────────────────────────────────────────┐
│          LESSON COMPLETION STATES                            │
└─────────────────────────────────────────────────────────────┘

  START
    ↓
┌──────────────┐  (watching video)  ┌──────────────┐
│ NOT_STARTED  │────────────────────→│  IN_PROGRESS │
└──────────────┘                     └──────────────┘
                                            │
                      (finished content)    │
                      (no assessment)       │
                                           ↓
                                      ┌──────────────┐
                                      │  COMPLETED   │
                                      └──────────────┘

  WITH ASSESSMENT:
  ┌──────────────┐  (watching)  ┌──────────────┐  (do quiz)
  │ NOT_STARTED  │────────────→│  IN_PROGRESS │──────────→
  └──────────────┘             └──────────────┘
                                                     ↓
                                    (score < passing)│ (score >= passing)
                                                     ↓
                                            ┌──────────────┐
                                            │   PASSED     │
                                            └──────────────┘
                                                   ↑
                                                   │
                                            ┌──────────────┐
                                            │   FAILED     │
                                            └──────────────┘
                                            (retry available)
```

---

## 📱 **4. Component & Interaction Details**

### 4.1 **Video Player Component**
- Auto-save watch progress
- Resume from last position
- Playback speed control (0.75x, 1x, 1.25x, 1.5x)
- Quality settings (360p, 480p, 720p, 1080p)
- Auto-mark as watched when 95% complete
- Fullscreen support

### 4.2 **Quiz Component**
- Question timer (countdown)
- Progress bar (Q1/5)
- Question review mode before submit
- Shuffle questions option
- One-question-per-page or all-on-one-page
- Answer validation before submit

### 4.3 **Progress Tracker**
- Chapter progress bar
- Lesson completion status icons
- Time spent counter
- Last activity timestamp
- Overall course progress %

### 4.4 **Comments Section**
- Threaded replies
- Upvote/Like feature
- Sort by newest/oldest/most helpful
- Pin comments (instructor only)
- Markdown support
- User avatars & roles

---

## 🎯 **5. Key UX Patterns**

### 5.1 **Locked Content**
```
┌──────────────────────────┐
│ 🔒 This lesson is locked │
├──────────────────────────┤
│ Complete "Bài 2: ..."    │
│ in Chapter 1 first       │
│                          │
│ [Go to prerequisite]     │
└──────────────────────────┘
```

### 5.2 **Empty States**
```
┌──────────────────────────────┐
│  📚 No courses yet            │
├──────────────────────────────┤
│ Start learning today! Browse  │
│ our course catalog and enroll │
│                               │
│ [Browse Courses]              │
└──────────────────────────────┘
```

### 5.3 **Loading States**
- Skeleton loaders for course cards
- Progress bar for video loading
- Spinner for assessment submission
- Disabled buttons during submission

### 5.4 **Error Handling**
```
❌ Error: Failed to submit assessment
Please check your connection and try again.
[Retry]
```

---

## 🎨 **6. Design System & Accessibility**

### Color Palette
- Primary: Blue (#2563eb)
- Success: Green (#10b981)
- Warning: Amber (#f59e0b)
- Danger: Red (#ef4444)
- Neutral: Gray (#6b7280)

### Typography
- Headings: 24px, 20px, 18px (Bold)
- Body: 16px (Regular)
- Small text: 14px (Regular)
- Captions: 12px (Regular)

### Spacing
- Standard: 8px, 16px, 24px, 32px, 48px
- Padding: 16px (default), 24px (cards)
- Margins: 16px (between sections)

### Accessibility
- WCAG 2.1 AA compliant
- Keyboard navigation support
- Screen reader friendly
- High contrast text
- Focus indicators on buttons/inputs

