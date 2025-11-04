// [Nội dung file Controllers/AccountController.cs]
using BTL_LTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

namespace BTL_LTW.Controllers
{
    public class AccountController : Controller
    {
        private readonly AssociationPortalContext _context;

        public AccountController(AssociationPortalContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Tìm user trong CSDL
            var user = await _context.Members
                             .FirstOrDefaultAsync(m => m.Emai == email);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // 2. Kiểm tra mật khẩu
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PassWordHash);

            if (!isValidPassword)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // 3. LẤY QUYỀN (ROLES) CỦA USER (THAY ĐỔI QUAN TRỌNG)
            var userPermissions = await _context.MemberPermisions
                                        .Where(mp => mp.MemberId == user.MembersId && mp.Licensed == true)
                                        .Include(mp => mp.Permision) // Join sang bảng Permision
                                        .ToListAsync();

            var roles = userPermissions.Select(mp => mp.Permision.PermisionName);

            // 4. TẠO COOKIE ĐĂNG NHẬP (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? "User"), 
                new Claim(ClaimTypes.Email, user.Emai),
                new Claim(ClaimTypes.NameIdentifier, user.MembersId.ToString())
            };

            // Thêm các quyền (roles) đã lấy được vào danh sách claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            // 5. Chuyển hướng về trang chủ
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(string fullname, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var existingUser = await _context.Members.FirstOrDefaultAsync(m => m.Emai == email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email này đã được sử dụng.";
                return View();
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var newUser = new Member
            {
                FullName = fullname,
                Emai = email,
                PassWordHash = hashedPassword,
                CreatedAt = DateTime.Now
            };

            _context.Members.Add(newUser);
            await _context.SaveChangesAsync();

            // Tự động gán quyền 'Member' (đây là quyền mặc định khi đăng ký)
            var memberRole = await _context.Permisions.FirstOrDefaultAsync(p => p.PermisionName == "Member");
            if (memberRole != null)
            {
                var newPermission = new MemberPermision
                {
                    MemberId = newUser.MembersId,
                    PermisionId = memberRole.PermisionId,
                    Licensed = true
                };
                _context.MemberPermisions.Add(newPermission);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Login");
        }

        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}