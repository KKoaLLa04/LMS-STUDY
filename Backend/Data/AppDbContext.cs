using System.Linq.Expressions;
using Backend.Common;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<KhoiHoc> KhoiHocs => Set<KhoiHoc>();
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<VirtualClassroom> VirtualClassrooms => Set<VirtualClassroom>();
    public DbSet<DiscussionPost> DiscussionPosts => Set<DiscussionPost>();
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<AchievementGroup> AchievementGroups => Set<AchievementGroup>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswerOption> QuizAttemptAnswerOptions => Set<QuizAttemptAnswerOption>();
    public DbSet<PointTransaction> PointTransactions => Set<PointTransaction>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<AchievementCondition> AchievementConditions => Set<AchievementCondition>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<DiscussionPostLike> DiscussionPostLikes => Set<DiscussionPostLike>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<TeacherModulePermission> TeacherModulePermissions => Set<TeacherModulePermission>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<PermissionGroupModulePermission> PermissionGroupModulePermissions => Set<PermissionGroupModulePermission>();
    public DbSet<UserPermissionGroup> UserPermissionGroups => Set<UserPermissionGroup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.Property(c => c.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(c => c.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            // Chỉ số thường trên KhoiHocId để truy vấn nhanh — không khai báo quan hệ
            // khóa ngoại (HasOne/WithMany), nên xóa KhoiHoc không bao giờ bị chặn.
            entity.HasIndex(c => c.KhoiHocId);
            // Cùng lý do như trên, áp dụng cho CategoryId.
            entity.HasIndex(c => c.CategoryId);
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.ToTable("CourseCategories");
            entity.HasIndex(c => c.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.ToTable("Sections");
            entity.HasOne(s => s.Course)
                  .WithMany(c => c.Sections)
                  .HasForeignKey(s => s.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("Lessons");
            entity.Property(l => l.LessonType)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(l => l.Section)
                  .WithMany(s => s.Lessons)
                  .HasForeignKey(l => l.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Document/Quiz dùng chung — nullable, không cascade: xóa Lesson không được xóa
            // Document/Quiz (có thể đang được Lesson khác dùng chung); DocumentService/
            // QuizLibraryService tự chặn xóa khi vẫn còn Lesson tham chiếu.
            entity.HasOne(l => l.Document)
                  .WithMany()
                  .HasForeignKey(l => l.DocumentId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(l => l.Quiz)
                  .WithMany()
                  .HasForeignKey(l => l.QuizId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.Property(d => d.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.Property(d => d.Status)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.ToTable("Quizzes");
            entity.Property(q => q.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<KhoiHoc>(entity =>
        {
            entity.ToTable("KhoiHocs");
            // Filtered index: mã chỉ cần duy nhất trong số các bản ghi chưa bị xóa mềm,
            // cho phép tái sử dụng mã sau khi khối học cũ đã bị xóa.
            entity.HasIndex(k => k.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<VirtualClassroom>(entity =>
        {
            entity.ToTable("VirtualClassrooms");
            entity.Property(v => v.Platform)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(v => v.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(v => v.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(v => v.Course)
                  .WithMany()
                  .HasForeignKey(v => v.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscussionPost>(entity =>
        {
            entity.ToTable("DiscussionPosts");
            entity.Property(p => p.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(p => p.Course)
                  .WithMany()
                  .HasForeignKey(p => p.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(p => p.ParentPost)
                  .WithMany(p => p.Replies)
                  .HasForeignKey(p => p.ParentPostId)
                  .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(p => p.User)
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ChatChannel>(entity =>
        {
            entity.ToTable("ChatChannels");
            entity.Property(c => c.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(c => c.Course)
                  .WithMany()
                  .HasForeignKey(c => c.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("ChatMessages");
            entity.Property(m => m.SentAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(m => m.Channel)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ChannelId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.Property(u => u.Status)
                  .HasConversion<short>();
            entity.Property(u => u.Gender)
                  .HasConversion<short?>();
            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasIndex(u => u.KhoiHocId);
        });

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.ToTable("Achievements");
            // Không khai báo quan hệ khóa ngoại tới AchievementGroup — cùng lý do như
            // Course.CategoryId: xóa nhóm huy hiệu không bao giờ bị chặn.
            entity.HasIndex(a => a.GroupId);
        });

        modelBuilder.Entity<AchievementGroup>(entity =>
        {
            entity.ToTable("AchievementGroups");
            entity.HasIndex(g => g.Code).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.Property(e => e.EnrolledAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Một học sinh chỉ ghi danh một lần vào một khóa học (trong số bản ghi chưa xóa mềm).
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.ToTable("UserAchievements");
            entity.Property(ua => ua.UnlockedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(ua => ua.User)
                  .WithMany()
                  .HasForeignKey(ua => ua.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(ua => ua.Achievement)
                  .WithMany()
                  .HasForeignKey(ua => ua.AchievementId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();
        });

        modelBuilder.Entity<AchievementCondition>(entity =>
        {
            entity.ToTable("AchievementConditions");
            entity.Property(c => c.ConditionType)
                  .HasConversion<string>()
                  .HasMaxLength(30);
            entity.HasOne(c => c.Achievement)
                  .WithMany()
                  .HasForeignKey(c => c.AchievementId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => c.AchievementId);
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            entity.ToTable("Follows");
            entity.Property(f => f.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            // Cả hai FK đều trỏ tới Users — chỉ được phép Cascade ở một phía, nếu không SQL Server
            // báo lỗi multiple cascade paths (cùng lý do như DiscussionPost.ParentPost dùng NoAction).
            entity.HasOne(f => f.Follower)
                  .WithMany()
                  .HasForeignKey(f => f.FollowerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(f => f.Following)
                  .WithMany()
                  .HasForeignKey(f => f.FollowingId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
        });

        modelBuilder.Entity<DiscussionPostLike>(entity =>
        {
            entity.ToTable("DiscussionPostLikes");
            entity.Property(l => l.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(l => l.Post)
                  .WithMany()
                  .HasForeignKey(l => l.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(l => l.User)
                  .WithMany()
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.ToTable("CourseReviews");
            entity.Property(r => r.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(r => r.Course)
                  .WithMany()
                  .HasForeignKey(r => r.CourseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Một học sinh chỉ có 1 đánh giá cho 1 khóa học (trong số bản ghi chưa xóa mềm).
            entity.HasIndex(r => new { r.CourseId, r.UserId }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<TeacherModulePermission>(entity =>
        {
            entity.ToTable("TeacherModulePermissions");
            entity.Property(p => p.Module)
                  .HasConversion<string>()
                  .HasMaxLength(30);
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(p => new { p.UserId, p.Module }).IsUnique();
        });

        modelBuilder.Entity<PermissionGroup>(entity =>
        {
            entity.ToTable("PermissionGroups");
        });

        modelBuilder.Entity<PermissionGroupModulePermission>(entity =>
        {
            entity.ToTable("PermissionGroupModulePermissions");
            entity.Property(p => p.Module)
                  .HasConversion<string>()
                  .HasMaxLength(30);
            entity.HasOne<PermissionGroup>()
                  .WithMany()
                  .HasForeignKey(p => p.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(p => new { p.GroupId, p.Module }).IsUnique();
        });

        modelBuilder.Entity<UserPermissionGroup>(entity =>
        {
            entity.ToTable("UserPermissionGroups");
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<PermissionGroup>()
                  .WithMany()
                  .HasForeignKey(p => p.GroupId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(p => new { p.UserId, p.GroupId }).IsUnique();
        });

        modelBuilder.Entity<LessonProgress>(entity =>
        {
            entity.ToTable("LessonProgresses");
            entity.Property(lp => lp.UpdatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(lp => lp.User)
                  .WithMany()
                  .HasForeignKey(lp => lp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(lp => lp.Lesson)
                  .WithMany()
                  .HasForeignKey(lp => lp.LessonId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(lp => new { lp.UserId, lp.LessonId }).IsUnique();
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.ToTable("QuizAttempts");
            entity.Property(qa => qa.AttemptedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(qa => qa.User)
                  .WithMany()
                  .HasForeignKey(qa => qa.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            // QuizId nullable — giữ lịch sử attempt kể cả khi không backfill được Quiz gốc.
            entity.HasOne(qa => qa.Quiz)
                  .WithMany()
                  .HasForeignKey(qa => qa.QuizId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuizAttemptAnswerOption>(entity =>
        {
            entity.ToTable("QuizAttemptAnswerOptions");
            // Xóa attempt thì xóa luôn chi tiết đáp án đã chọn của attempt đó.
            entity.HasOne(a => a.QuizAttempt)
                  .WithMany()
                  .HasForeignKey(a => a.QuizAttemptId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(a => a.QuizAttemptId);
        });

        modelBuilder.Entity<PointTransaction>(entity =>
        {
            entity.ToTable("PointTransactions");
            entity.Property(pt => pt.SourceType)
                  .HasConversion<string>()
                  .HasMaxLength(30);
            entity.Property(pt => pt.CreatedAt)
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .ValueGeneratedOnAdd();
            entity.HasOne(pt => pt.User)
                  .WithMany()
                  .HasForeignKey(pt => pt.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            // Không khai báo FK tới Course — cùng lý do như Course.KhoiHocId: điểm lịch sử
            // không nên bị chặn xóa hay bị xóa cascade nếu khóa học bị gỡ sau này.
            entity.HasIndex(pt => pt.CourseId);
            entity.HasIndex(pt => new { pt.UserId, pt.CreatedAt });
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.ToTable("QuizQuestions");
            entity.HasOne(q => q.Quiz)
                  .WithMany(q => q.Questions)
                  .HasForeignKey(q => q.QuizId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(q => q.QuizId);
        });

        modelBuilder.Entity<QuizOption>(entity =>
        {
            entity.ToTable("QuizOptions");
            entity.HasOne(o => o.Question)
                  .WithMany(q => q.Options)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(o => o.QuestionId);
        });

        // Xóa mềm: tự động loại bỏ các bản ghi có IsDeleted = true khỏi mọi truy vấn LINQ,
        // áp dụng cho toàn bộ entity implement ISoftDelete mà không cần lặp lại Where() ở từng service.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)) continue;

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
            var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var lambda = Expression.Lambda(notDeleted, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
