using BTL_LTW.Models;
using Microsoft.AspNetCore.Authentication.Cookies; // <-- THÊM CÁI NÀY
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Cấu hình DbContext
builder.Services.AddDbContext<AssociationPortalContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Cấu hình Dịch vụ Xác thực Cookie (BẮT BUỘC)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Account/Login"; // Trang bị từ chối (do thiếu quyền)
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie hết hạn sau 7 ngày
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Kích hoạt Xác thực và Phân quyền (BẮT BUỘC)
app.UseAuthentication(); // <-- BẬT XÁC THỰC
app.UseAuthorization();  // <-- BẬT PHÂN QUYỀN

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();