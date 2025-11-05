// ...existing code...
using BTL_LTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims; // <-- THÊM CÁI NÀY
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace BTL_LTW.Controllers
{
    public class AccountController : Controller
    {
        private readonly AssociationPortalContext _context;

        public AccountController(AssociationPortalContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Members
                             .FirstOrDefaultAsync(m => m.Emai == email);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PassWordHash ?? "");

            if (!isValidPassword)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // 3. LẤY QUYỀN (ROLES) CỦA USER
            var userPermissions = await _context.MemberPermisions
                                        .Where(mp => mp.MemberId == user.MembersId && mp.Licensed == true)
                                        .Include(mp => mp.Permision)
                                        .ToListAsync();

            var roles = userPermissions.Select(mp => mp.Permision.PermisionName);

            // 4. TẠO COOKIE ĐĂNG NHẬP (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? "User"),
                new Claim(ClaimTypes.Email, user.Emai ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.MembersId.ToString())
            };

            // Thêm các quyền (roles) đã lấy được vào danh sách claims
            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

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

            // Tự động gán quyền 'Member'
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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Action này hiển thị trang xác nhận
        // Yêu cầu người dùng phải đăng nhập mới thấy được
        [Authorize]
        [HttpGet]
        public IActionResult RegisterEnterprise()
        {
            // Chỉ trả về 1 View đơn giản để xác nhận
            return View();
        }

        // Action này xử lý logic khi người dùng bấm nút "Xác nhận"
        [Authorize]
        [HttpPost, ActionName("RegisterEnterprise")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEnterprisePost()
        {
            // 1. Lấy email của người dùng đang đăng nhập
           // Dòng này ĐÚNG
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            // 2. Tìm Member trong database (sử dụng Emai vì model dùng tên này)
            var member = await _context.Members.FirstOrDefaultAsync(m => m.Emai == userEmail);
            if (member == null)
            {
                return NotFound("Không tìm thấy tài khoản.");
            }

            // 3. Tìm Role "Enterprise"
            var enterpriseRole = await _context.Permisions.FirstOrDefaultAsync(p => p.PermisionName == "Enterprise");
            if (enterpriseRole == null)
            {
                ViewBag.Error = "Lỗi hệ thống: Không tìm thấy vai trò Doanh nghiệp.";
                return View();
            }

            // 4. Kiểm tra xem người dùng đã có role này chưa
            var hasRole = await _context.MemberPermisions.AnyAsync(mp =>
                mp.MemberId == member.MembersId &&
                mp.PermisionId == enterpriseRole.PermisionId
            );

            if (hasRole)
            {
                ViewBag.Error = "Tài khoản của bạn đã là Doanh nghiệp.";
                return View();
            }

            // 5. Thêm role "Enterprise" cho người dùng
            var newMemberPermission = new MemberPermision
            {
                MemberId = member.MembersId,
                PermisionId = enterpriseRole.PermisionId,
                Licensed = true
            };

            _context.MemberPermisions.Add(newMemberPermission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nâng cấp tài khoản lên Doanh nghiệp thành công! Vui lòng đăng nhập lại để cập nhật quyền của bạn.";

            // Đăng xuất người dùng hiện tại để cập nhật claims khi đăng nhập lại
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }
    }
}
// ...existing code...