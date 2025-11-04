// [Nội dung file Controllers/PostController.cs]
using BTL_LTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Cần thêm cái này
using System;

namespace BTL_LTW.Controllers
{
    // Yêu cầu đăng nhập và có 1 trong 3 quyền này
    [Authorize(Roles = "Admin, Enterprise, Moderator")]
    public class PostController : Controller
    {
        private readonly AssociationPortalContext _context;

        public PostController(AssociationPortalContext context)
        {
            _context = context;
        }

        // GET: /Post/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách Categories để đưa vào thẻ <select>
            var categories = await _context.Categories
                                        .OrderBy(c => c.CategoryName)
                                        .ToListAsync();
            
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: /Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostTitle,PostContent,PostThumbnailUrl,CategoryId")] Post post)
        {
            var memberIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(memberIdStr, out int memberId))
            {
                return Unauthorized(); 
            }
            
            // Xóa lỗi validate của các thuộc tính chúng ta không bind
            ModelState.Remove("Member");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                post.MemberId = memberId;
                post.CreatedAt = DateTime.Now;
                post.UpdatedAt = DateTime.Now;
                
                // QUAN TRỌNG: Đặt trạng thái chờ duyệt
                post.ApproveStatus = 0; // 0 = Chờ duyệt
                post.PostStatus = 0;    // 0 = Ẩn (vì chưa được duyệt)
                post.ViewCount = 0;
                
                // Nếu người đăng là Admin, tự động duyệt
                if (User.IsInRole("Admin"))
                {
                    post.ApproveStatus = 1; // 1 = Đã duyệt
                    post.PostStatus = 1;    // 1 = Hiển thị
                    post.ApproveBy = memberId;
                    post.ApprovedDate = DateTime.Now;
                }

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                // Chuyển hướng đến trang Admin để xem bài chờ (nếu không phải Admin)
                if (post.ApproveStatus == 0)
                {
                    TempData["SuccessMessage"] = "Đăng bài thành công! Bài viết của bạn đang chờ duyệt.";
                    return RedirectToAction("Index", "Home");
                }
                
                // Nếu là Admin tự duyệt, chuyển đến bài viết
                return RedirectToAction("Details", "News", new { id = post.PostId });
            }

            // Nếu model không hợp lệ, tải lại danh sách categories
            var categories = await _context.Categories
                                        .OrderBy(c => c.CategoryName)
                                        .ToListAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", post.CategoryId);
            return View(post);
        }
    }
}