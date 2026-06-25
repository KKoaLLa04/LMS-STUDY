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
    public DbSet<VirtualClassroom> VirtualClassrooms => Set<VirtualClassroom>();
    public DbSet<DiscussionPost> DiscussionPosts => Set<DiscussionPost>();
    public DbSet<ChatChannel> ChatChannels => Set<ChatChannel>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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
    }
}
