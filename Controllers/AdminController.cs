// [Nội dung file Controllers/AdminController.cs]
using BTL_LTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BTL_LTW.Controllers
{
    // Yêu cầu đăng nhập và có 1 trong 2 quyền này
    [Authorize(Roles = "Admin, Moderator")]
    public class AdminController : Controller
    {
        private readonly AssociationPortalContext _context;

        public AdminController(AssociationPortalContext context)
        {
            _context = context;
        }

        // GET: /Admin (Dashboard chung)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/PendingPosts
        // Trang duyệt bài (cho cả Admin và Moderator)
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> PendingPosts()
        {
            var pendingList = await _context.Posts
                .Where(p => p.ApproveStatus == 0) // Lấy bài chờ duyệt
                .Include(p => p.Member)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            
            return View(pendingList);
        }

        // POST: /Admin/Approve/5
        // Action duyệt bài
        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(memberIdStr, out int adminId);

            post.ApproveStatus = 1; // 1 = Đã duyệt
            post.PostStatus = 1;    // 1 = Hiển thị
            post.ApprovedDate = DateTime.Now;
            post.ApproveBy = adminId;
            post.UpdatedAt = DateTime.Now;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("PendingPosts");
        }

        // POST: /Admin/Reject/5
        // Action từ chối bài
        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(memberIdStr, out int adminId);

            post.ApproveStatus = 2; // 2 = Đã từ chối
            post.PostStatus = 0;    // 0 = Ẩn
            post.ApprovedDate = DateTime.Now;
            post.ApproveBy = adminId;
            post.RejectedReason = reason; // Lưu lý do từ chối
            post.UpdatedAt = DateTime.Now;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("PendingPosts");
        }


        // GET: /Admin/ManageUsers
        // Trang quản lý người dùng (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Members
                .Include(m => m.MemberPermisions)
                .ThenInclude(mp => mp.Permision)
                .ToListAsync();
                
            return View(users);
        }
    }
}