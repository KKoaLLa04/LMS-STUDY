using Backend.Common;
using Backend.Configuration;
using Backend.Data;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Services.MeetingProviders;
using Backend.Services.VirtualClassrooms;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────────
// 1. Đăng ký các services
// ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Trả về format ApiResponse thống nhất khi model validation thất bại
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er => er.ErrorMessage))
                .ToList();

            var response = ApiResponse<object?>.BadRequest(string.Join("; ", errors));
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS – cho phép Angular dev server gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Đăng ký Application Services
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ILessonService, LessonService>();

// Virtual Classrooms & Discussion & Chat
builder.Services.AddScoped<IVirtualClassroomService, VirtualClassroomService>();
builder.Services.AddScoped<IDiscussionForumService, DiscussionForumService>();
builder.Services.AddScoped<IChatChannelService, ChatChannelService>();

// Meeting Providers (Zoom, Google Meet)
builder.Services.Configure<MeetingProviderOptions>(
    builder.Configuration.GetSection("MeetingProviders"));

builder.Services.AddHttpClient("ZoomApi");
builder.Services.AddHttpClient("ZoomToken");
builder.Services.AddSingleton<ZoomTokenCache>();
builder.Services.AddScoped<MeetingProviderFactory>();

// Đăng ký Zoom provider — dùng Fake khi chưa có credentials thật
var zoomEnabled = builder.Configuration.GetValue<bool>("MeetingProviders:Zoom:Enabled");
if (zoomEnabled)
    builder.Services.AddKeyedScoped<IMeetingProviderService, ZoomMeetingService>("Zoom");
else
    builder.Services.AddKeyedScoped<IMeetingProviderService, FakeMeetingProviderService>("Zoom");

// Đăng ký GoogleMeet provider — dùng Fake cho đến khi tích hợp đầy đủ
var googleEnabled = builder.Configuration.GetValue<bool>("MeetingProviders:GoogleMeet:Enabled");
if (googleEnabled)
    builder.Services.AddKeyedScoped<IMeetingProviderService, GoogleMeetService>("GoogleMeet");
else
    builder.Services.AddKeyedScoped<IMeetingProviderService, FakeMeetingProviderService>("GoogleMeet");

// Cấu hình SQL Server với EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ──────────────────────────────────────────────────────────────
// 2. Build ứng dụng
// ──────────────────────────────────────────────────────────────
var app = builder.Build();

// ──────────────────────────────────────────────────────────────
// 3. Tự động chạy migration khi ứng dụng khởi động
// ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        await db.Database.MigrateAsync();
        logger.LogInformation("Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }
}

// ──────────────────────────────────────────────────────────────
// 4. Cấu hình HTTP pipeline
// ──────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
