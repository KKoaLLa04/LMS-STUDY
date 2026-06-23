# 📋 Implementation Guide & Best Practices

## 🎯 **Tóm tắt thiết kế toàn hệ thống**

Bạn đã có đầy đủ tài liệu để bắt đầu:

### 📊 **1. Database Schema** (`course-management-schema.md`)
- 14 entities chính
- Relationships định nghĩa rõ (1:N, 1:1)
- Primary keys, foreign keys, unique constraints
- Business rules & validation

### 🔌 **2. API Endpoints** (`api-endpoints.md`)
- 12 nhóm endpoints chính
- RESTful conventions
- Request/response examples (JSON)
- Error handling & pagination
- Authentication (JWT Bearer tokens)

### 🎨 **3. UI/UX Flow** (`ui-ux-flow.md`)
- User journey maps (Student, Instructor, Admin)
- Screen wireframes (8+ screens chi tiết)
- State transitions & workflows
- Component patterns
- Design system (colors, typography, spacing)

---

## 🛠️ **Implementation Roadmap (5 Phases)**

### **Phase 0: Foundation (Week 1-2)**

#### Database Setup
```bash
# 1. Tạo PostgreSQL database
createdb coursepro

# 2. Run migration scripts (thứ tự)
-- 1. Users (base entity)
-- 2. Categories
-- 3. Courses (+ foreign keys to users, categories)
-- 4. Chapters (+ FK to courses)
-- 5. Lessons (+ FK to chapters)
-- 6. Assessments (+ FK to lessons)
-- 7. Assessment_questions (+ FK to assessments)
-- 8. Enrollments (+ FK to users, courses)
-- 9. Lesson_progress (+ FK to users, lessons)
-- 10. Assessment_results (+ FK to users, assessments)
-- 11. Student_answers (+ FK to users, assessments, questions)
-- 12. Lesson_comments (+ FK to lessons, users)
-- 13. Lesson_resources (+ FK to lessons)
-- 14. Certificates (+ FK to users, courses)

# 3. Seed dữ liệu demo (categories, test users)
```

#### ASP.NET Core 8 Project Setup
```bash
# 1. Tạo solution
dotnet new sln -n CoursePro
dotnet new webapi -n CoursePro.API
dotnet new classlib -n CoursePro.Data
dotnet new classlib -n CoursePro.Domain
dotnet new classlib -n CoursePro.Services

# 2. Install dependencies
dotnet add CoursePro.API package EntityFrameworkCore
dotnet add CoursePro.API package EntityFrameworkCore.Npgsql
dotnet add CoursePro.API package AutoMapper
dotnet add CoursePro.API package FluentValidation
dotnet add CoursePro.API package Serilog
dotnet add CoursePro.API package StackExchange.Redis

# 3. Configure appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=coursepro;Username=postgres;Password=..."
  },
  "Redis": {
    "Connection": "localhost:6379"
  }
}

# 4. Setup DbContext + migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

### **Phase 1: Core APIs (Week 3-5)**

#### Priority Endpoints
1. **Authentication**
   - `POST /auth/register` - Đăng ký
   - `POST /auth/login` - Đăng nhập
   - `POST /auth/refresh-token` - Refresh JWT token

2. **Courses (CRUD)**
   - `GET /courses` - Danh sách (public)
   - `GET /courses/{id}` - Chi tiết
   - `POST /courses` - Tạo (Instructor only)
   - `PUT /courses/{id}` - Cập nhật draft
   - `POST /courses/{id}/publish` - Công bố

3. **Chapters & Lessons**
   - CRUD operations cho chapters
   - CRUD operations cho lessons
   - `POST /lessons/{id}/progress` - Cập nhật tiến độ

4. **Enrollment**
   - `POST /courses/{id}/enroll` - Đăng ký
   - `GET /courses/{id}/enrollments` - Danh sách học sinh
   - `GET /my-courses` - Khóa học của học sinh

#### Implementation Pattern (ASP.NET Core)
```csharp
// 1. Controllers (Accept requests)
[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    
    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourse(Guid id)
    {
        var course = await _courseService.GetCourseAsync(id);
        return Ok(course);
    }
    
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<ActionResult> CreateCourse(CreateCourseRequest request)
    {
        // Validate input
        var validator = new CreateCourseValidator();
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        var result = await _courseService.CreateCourseAsync(request, UserId);
        return CreatedAtAction(nameof(GetCourse), new { id = result.Id }, result);
    }
}

// 2. Services (Business logic)
public interface ICourseService
{
    Task<CourseDto> GetCourseAsync(Guid id);
    Task<CreateCourseDto> CreateCourseAsync(CreateCourseRequest request, Guid instructorId);
    Task UpdateCourseAsync(Guid id, UpdateCourseRequest request, Guid instructorId);
    Task PublishCourseAsync(Guid id, Guid instructorId);
}

// 3. DTOs (Data Transfer Objects)
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public int Version { get; set; }
}

// 4. Repository (Data access)
public interface ICourseRepository
{
    Task<Course> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetPublishedAsync(int page, int limit);
    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
}
```

---

### **Phase 2: Learning Features (Week 6-8)**

#### Key Implementations
1. **Lesson Progress Tracking**
   - Video watch tracking (auto-mark at 95% watched)
   - Save progress periodically (every 30 seconds)
   - Resume from last position

2. **Comments System**
   - Threaded replies
   - Upvote/like feature
   - Pin comments (Instructor only)

3. **Quiz/Assessment**
   - Take quiz endpoint
   - Auto-grading (multiple choice)
   - Manual review for essays

#### Database Optimization
```sql
-- Add indexes for better performance
CREATE INDEX idx_enrollments_student_course 
  ON student_enrollments(student_id, course_id);

CREATE INDEX idx_lesson_progress_student 
  ON lesson_progress(student_id) 
  WHERE status != 'not_started';

CREATE INDEX idx_comments_lesson_created 
  ON lesson_comments(lesson_id, created_at DESC);
```

---

### **Phase 3: Angular Frontend (Week 9-12)**

#### Component Architecture
```
src/
├── app/
│   ├── shared/
│   │   ├── components/        # Shared UI (Header, Footer, VideoPlayer, etc)
│   │   ├── services/          # API, Auth, Cache services
│   │   └── models/            # DTOs, Interfaces
│   │
│   ├── student/
│   │   ├── components/
│   │   │   ├── course-list/
│   │   │   ├── course-detail/
│   │   │   ├── lesson-player/
│   │   │   ├── quiz-player/
│   │   │   └── dashboard/
│   │   ├── services/          # StudentService
│   │   └── student.module.ts
│   │
│   ├── instructor/
│   │   ├── components/
│   │   │   ├── course-editor/
│   │   │   ├── chapter-editor/
│   │   │   ├── lesson-editor/
│   │   │   ├── quiz-builder/
│   │   │   ├── student-management/
│   │   │   └── analytics-dashboard/
│   │   ├── services/          # InstructorService
│   │   └── instructor.module.ts
│   │
│   └── admin/
│       ├── components/
│       │   ├── course-moderation/
│       │   ├── user-management/
│       │   └── system-analytics/
│       ├── services/          # AdminService
│       └── admin.module.ts
│
└── environments/
```

#### Key Angular Services
```typescript
// auth.service.ts
@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenSubject = new BehaviorSubject<string | null>(null);
  
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/auth/login', { email, password });
  }
  
  logout(): void { /* ... */ }
}

// course.service.ts
@Injectable({ providedIn: 'root' })
export class CourseService {
  constructor(private http: HttpClient, private cache: CacheService) {}
  
  getCourses(page: number, limit: number): Observable<CoursePage> {
    return this.cache.get('courses', () => 
      this.http.get<CoursePage>(`/courses?page=${page}&limit=${limit}`)
    );
  }
  
  getCourseDetail(id: string): Observable<Course> {
    return this.http.get<Course>(`/courses/${id}`);
  }
}

// lesson.service.ts
@Injectable({ providedIn: 'root' })
export class LessonService {
  updateProgress(lessonId: string, progress: number): Observable<void> {
    return this.http.post<void>(
      `/lessons/${lessonId}/progress`,
      { status: 'in_progress', completionPercentage: progress }
    ).pipe(
      debounceTime(5000), // Save every 5 seconds
      distinctUntilChanged()
    );
  }
}
```

#### Video Player Component
```typescript
@Component({
  selector: 'app-video-player',
  template: `
    <div class="player">
      <video #videoElement
             (timeupdate)="onTimeUpdate()"
             (ended)="onVideoEnd()">
        <source [src]="videoUrl" type="video/mp4">
      </video>
      
      <div class="controls">
        <button (click)="togglePlay()">▶️/⏸️</button>
        <div class="progress">
          <input type="range" [value]="currentTime" [max]="duration"
                 (change)="seek($event)">
        </div>
        <span>{{ currentTime | timeformat }} / {{ duration | timeformat }}</span>
        <select [(ngModel)]="playbackRate">
          <option value="0.75">0.75x</option>
          <option value="1">1x</option>
          <option value="1.25">1.25x</option>
          <option value="1.5">1.5x</option>
        </select>
      </div>
    </div>
  `
})
export class VideoPlayerComponent implements OnInit, OnDestroy {
  @Input() videoUrl: string;
  @Input() lessonId: string;
  
  @ViewChild('videoElement') videoElement!: HTMLVideoElement;
  
  currentTime = 0;
  duration = 0;
  private updateSubscription: Subscription;
  
  constructor(private lessonService: LessonService) {}
  
  onTimeUpdate(): void {
    this.currentTime = this.videoElement.currentTime;
    // Auto-save progress (debounced)
    this.lessonService.updateProgress(this.lessonId, 
      (this.currentTime / this.duration) * 100
    ).subscribe();
  }
  
  onVideoEnd(): void {
    // Mark as watched + completed
    this.lessonService.markAsCompleted(this.lessonId).subscribe();
  }
}
```

---

### **Phase 4: Advanced Features (Week 13-15)**

1. **Analytics Dashboard**
   - Chart.js integration
   - Real-time metrics
   - Export to CSV/PDF

2. **Notifications**
   - Email notifications (SendGrid/Mailgun)
   - In-app notifications
   - Comment notifications

3. **Certificates**
   - PDF generation (jsPDF)
   - QR code for verification
   - Download & sharing

4. **Performance Optimization**
   - Lazy loading
   - Code splitting
   - Image optimization
   - Caching strategies

---

### **Phase 5: Deployment & Testing (Week 16)**

#### Unit Testing (xUnit / NUnit)
```csharp
[Fact]
public async Task CreateCourse_WithValidData_ReturnsCourseId()
{
    // Arrange
    var request = new CreateCourseRequest { Title = "Test Course" };
    var instructorId = Guid.NewGuid();
    
    // Act
    var result = await _courseService.CreateCourseAsync(request, instructorId);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("draft", result.Status);
}
```

#### Integration Testing
```csharp
[Fact]
public async Task GetCourse_ReturnsOkWithCourseData()
{
    // Arrange
    var courseId = await SeedTestCourse();
    
    // Act
    var response = await _client.GetAsync($"/api/courses/{courseId}");
    
    // Assert
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var course = JsonConvert.DeserializeObject<CourseDto>(json);
    Assert.NotNull(course);
}
```

#### Deployment
```bash
# Docker
docker build -t coursepro:latest .
docker run -p 5000:5000 coursepro:latest

# Kubernetes (optional)
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Database Migration
dotnet ef database update -e Production
```

---

## 🏗️ **Technology Stack Summary**

| Layer | Technology | Rationale |
|-------|-----------|-----------|
| Frontend | Angular 18 | Component-based, type-safe |
| Backend | ASP.NET Core 8 | Enterprise, performance, C# |
| Database | PostgreSQL 14 | ACID, relationships, scalable |
| Cache | Redis | Session, course data cache |
| File Storage | S3 / Cloudinary | Video, PDF, images |
| API Style | RESTful + JWT | Standard, stateless |
| Testing | xUnit | .NET standard |
| Deployment | Docker + K8s | Scalability, containers |

---

## 📌 **Best Practices & Recommendations**

### ✅ **Database**
- Use transactions cho multi-step operations (publish course, enroll student)
- Add audit columns (created_at, updated_at, created_by)
- Soft deletes cho sensitive data (courses, users)
- Regular backups

### ✅ **API Design**
- Consistent error responses với status codes
- Versioning (`/api/v1/courses`)
- Rate limiting để prevent abuse
- CORS configuration
- Request validation trước business logic

### ✅ **Security**
- Hash passwords (BCrypt, not MD5)
- JWT token expiry (15 min access, 7 day refresh)
- Input sanitization (SQL injection, XSS)
- Authorization checks (role-based)
- HTTPS only
- Environment variables cho secrets

### ✅ **Performance**
- Database indexing trên frequently queried columns
- Query optimization (N+1 prevention với eager loading)
- Pagination cho list endpoints
- Caching strategy (Redis)
- Lazy loading components (Angular)
- Code splitting bundles

### ✅ **Testing**
- Unit tests cho services (80%+ coverage)
- Integration tests cho API endpoints
- E2E tests cho critical flows
- Automated testing pipeline (CI/CD)

### ✅ **Monitoring**
- Logging (Serilog) - levels: Info, Warning, Error
- Error tracking (Sentry)
- Performance monitoring (New Relic)
- Database query profiling

---

## 🚀 **Getting Started Checklist**

- [ ] Clone repository
- [ ] Setup PostgreSQL locally
- [ ] Run migrations
- [ ] Seed test data
- [ ] Configure appsettings.json
- [ ] Run ASP.NET Core API (dotnet run)
- [ ] Setup Angular environment
- [ ] Install npm dependencies (npm install)
- [ ] Run Angular dev server (ng serve)
- [ ] Test authentication flow
- [ ] Create first course (Instructor)
- [ ] Enroll as student
- [ ] Play lesson & quiz

---

## 📞 **Support & Questions**

**Common Issues:**

1. **Connection String Error**
   ```
   Solution: Verify PostgreSQL is running, password is correct
   ```

2. **CORS Error**
   ```csharp
   // Add to Program.cs
   services.AddCors(options => {
       options.AddPolicy("AllowAngular", policy => 
           policy.WithOrigins("http://localhost:4200"));
   });
   ```

3. **JWT Token Expired**
   ```
   Solution: Implement token refresh endpoint, update localStorage
   ```

4. **Database Migration Failed**
   ```
   Solution: dotnet ef database drop --force && dotnet ef database update
   ```

---

## 📚 **Additional Resources**

- [ASP.NET Core Documentation](https://docs.microsoft.com/dotnet)
- [Angular Documentation](https://angular.io)
- [PostgreSQL Documentation](https://www.postgresql.org/docs)
- [Entity Framework Core Guide](https://docs.microsoft.com/ef/core)
- [JWT Best Practices](https://tools.ietf.org/html/rfc7519)

---

**Trên đây là bộ tài liệu hoàn chỉnh để bắt đầu xây dựng hệ thống quản lý khóa học. Good luck! 🚀**

