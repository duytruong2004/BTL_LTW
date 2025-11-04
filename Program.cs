using Microsoft.EntityFrameworkCore;
using BTL_LTW.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// 🔹 Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔹 Đăng ký DbContext trước khi Build()
builder.Services.AddDbContext<AssociationPortalContext>(options =>
    options.UseSqlServer(connectionString));

 builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// 🔹 Thêm Controller + View
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 🔹 Cấu hình pipeline xử lý HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // <-- XÁC THỰC
app.UseAuthorization();  // <-- PHÂN QUYỀN

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
