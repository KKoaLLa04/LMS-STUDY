using System.Text;
using Backend.Common;
using Backend.Configuration;
using Backend.Data;
using Backend.Models;
using Backend.Services;
using Backend.Services.Interfaces;
using Backend.Services.MeetingProviders;
using Backend.Services.VirtualClassrooms;
using Backend.Services.VirtualClassrooms.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token dạng: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

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
builder.Services.AddScoped<IKhoiHocService, KhoiHocService>();
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Cho phép upload video kích thước lớn (tối đa 500MB) qua multipart/form-data
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500 * 1024 * 1024;
});

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

// Authentication (JWT) & Authorization
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Jwt:Key chưa được cấu hình. Chạy: dotnet user-secrets set \"Jwt:Key\" \"<chuỗi bí mật>\" trong thư mục Backend.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

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

    // Seed tài khoản admin mặc định nếu chưa có
    if (!await db.Users.AnyAsync(u => u.Username == "admin"))
    {
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        logger.LogInformation("Đã seed tài khoản admin mặc định (username: admin).");
    }
}

// ──────────────────────────────────────────────────────────────
// 4. Cấu hình HTTP pipeline
// ──────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
