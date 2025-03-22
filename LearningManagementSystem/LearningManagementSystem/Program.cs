using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();

var app = builder.Build();

// Tạo tài khoản Admin mặc định
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LMSContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();

        // Đảm bảo cơ sở dữ liệu đã được tạo
        context.Database.EnsureCreated();

        // Kiểm tra xem có vai trò "Admin" chưa
        var adminRole = context.Roles.FirstOrDefault(r => r.RoleName == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                RoleId = "role-admin",
                RoleName = "Admin"
            };
            context.Roles.Add(adminRole);
            context.SaveChanges();
        }

        // Kiểm tra xem có tài khoản Admin nào chưa
        var adminUser = context.Users.FirstOrDefault(u => u.RoleId == adminRole.RoleId);
        if (adminUser == null)
        {
            var admin = new User
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = "admin",
                FullName = "Administrator",
                Email = "admin@example.com",
                RoleId = adminRole.RoleId
            };

            // Băm mật khẩu mặc định
            admin.HashPassword(passwordHasher, "Admin@123");

            context.Users.Add(admin);
            context.SaveChanges();

            Console.WriteLine("Tài khoản Admin đã được tạo: Username = admin, Password = Admin@123");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi tạo tài khoản Admin: {ex.Message}");
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();