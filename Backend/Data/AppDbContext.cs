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
    }
}
