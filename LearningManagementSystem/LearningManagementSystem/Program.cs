using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Đăng ký DbContext
builder.Services.AddDbContext<LMSContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký IPasswordHasher<User>
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Đăng ký Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Đăng ký các repository
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();

// Thêm hỗ trợ session (nếu cần cho giỏ hàng hoặc các tính năng khác)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Tạo tài khoản Admin và vai trò mặc định
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<LMSContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

        // Đảm bảo cơ sở dữ liệu đã được tạo
        context.Database.EnsureCreated();

        // Kiểm tra và tạo vai trò "Admin"
        var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                RoleId = "role-admin",
                RoleName = "Admin"
            };
            context.Roles.Add(adminRole);
            logger.LogInformation("Vai trò Admin đã được tạo.");
        }

        // Kiểm tra và tạo vai trò "Student"
        var studentRole = context.Roles.FirstOrDefault(r => r.RoleName == "Student");
        if (studentRole == null)
        {
            studentRole = new Role
            {
                RoleId = "role-student",
                RoleName = "Student"
            };
            context.Roles.Add(studentRole);
            logger.LogInformation("Vai trò Student đã được tạo.");
        }

        // Lưu các vai trò vào cơ sở dữ liệu
        context.SaveChanges();

        // Kiểm tra và tạo tài khoản Admin
        var adminUser = context.Users.FirstOrDefault(u => u.RoleId == adminRole.RoleId);
        if (adminUser == null)
        {
            var admin = new User
            { 
                UserName = "admin",
                FullName = "Administrator",
                Email = "admin@example.com",
                RoleId = adminRole.RoleId
            };

            // Băm mật khẩu mặc định
            admin.HashPassword(passwordHasher, "Admin@123");

            context.Users.Add(admin);
            context.SaveChanges();

            logger.LogInformation("Tài khoản Admin đã được tạo: Username = admin, Password = Admin@123");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi tạo tài khoản Admin hoặc vai trò mặc định.");
        throw; 
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Thêm middleware Authentication
app.UseAuthorization();

app.UseSession(); // Thêm middleware Session

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();