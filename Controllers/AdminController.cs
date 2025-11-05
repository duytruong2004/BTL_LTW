using BTL_LTW.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace BTL_LTW.Controllers
{
    [Authorize(Roles = "Admin, Moderator")]
    public class AdminController : Controller
    {
        private readonly AssociationPortalContext _context;

        public AdminController(AssociationPortalContext context)
        {
            _context = context;
        }

        // Trang Dashboard
        public IActionResult Index()
        {
            return View();
        }

        // =================== DUYỆT BÀI ===================
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> PendingPosts()
        {
            var pendingList = await _context.Posts
                .Where(p => p.ApproveStatus == 0)
                .Include(p => p.Member)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(pendingList);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(memberIdStr, out int adminId);

            post.ApproveStatus = 1;
            post.PostStatus = 1;
            post.ApprovedDate = DateTime.Now;
            post.ApproveBy = adminId;
            post.UpdatedAt = DateTime.Now;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingPosts));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(memberIdStr, out int adminId);

            post.ApproveStatus = 2;
            post.PostStatus = 0;
            post.ApprovedDate = DateTime.Now;
            post.ApproveBy = adminId;
            post.RejectedReason = reason;
            post.UpdatedAt = DateTime.Now;

            _context.Update(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingPosts));
        }

        // =================== QUẢN LÝ NGƯỜI DÙNG ===================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Members
                .Include(m => m.MemberPermisions)
                .ThenInclude(mp => mp.Permision)
                .ToListAsync();

            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserRole(int? id)
        {
            if (id == null) return NotFound();

            var member = await _context.Members
                .Include(m => m.MemberPermisions)
                .FirstOrDefaultAsync(m => m.MembersId == id);

            if (member == null) return NotFound();

            ViewBag.Permissions = await _context.Permisions.ToListAsync();
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUserRole(int memberId, int permissionId)
        {
            var memberPermission = await _context.MemberPermisions
                .FirstOrDefaultAsync(mp => mp.MemberId == memberId); // Sửa lại đúng property

            if (memberPermission != null)
            {
                memberPermission.PermisionId = permissionId;
                _context.Update(memberPermission);
            }
            else
            {
                memberPermission = new MemberPermision
                {
                    MemberId = memberId,
                    PermisionId = permissionId
                };
                _context.Add(memberPermission);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageUsers));
        }


        // =================== QUẢN LÝ BÀI VIẾT ===================

[Authorize(Roles = "Admin, Moderator")]
public async Task<IActionResult> ManagePosts()
{
    // Lấy tất cả bài viết (bất kể trạng thái duyệt)
    var allPosts = await _context.Posts
        .Include(p => p.Member)      // Lấy thông tin người đăng
        .Include(p => p.Category)    // Lấy thông tin danh mục
        .OrderByDescending(p => p.CreatedAt) // Sắp xếp mới nhất lên đầu
        .ToListAsync();

    return View(allPosts);
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin, Moderator")]
public async Task<IActionResult> DeletePost(int id)
{
    var post = await _context.Posts.FindAsync(id);
    if (post == null)
    {
        return NotFound();
    }

    _context.Posts.Remove(post);
    await _context.SaveChangesAsync();

    // (Tùy chọn) Thêm TempData để thông báo thành công
    TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";

    return RedirectToAction(nameof(ManagePosts));
}
}
}
