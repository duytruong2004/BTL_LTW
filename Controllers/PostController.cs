using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BTL_LTW.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

namespace BTL_LTW.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly AssociationPortalContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostController(AssociationPortalContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            // Lấy tất cả category (bỏ filter CategoryType vì model không có)
            var categories = await _context.Categories
                                .OrderBy(c => c.CategoryName)
                                .ToListAsync();

            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostTitle,PostContent,CategoryId,PostThumbnailUrl")] Post post, IFormFile? ImageFile)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int memberId))
            {
                return Unauthorized("Không thể xác thực người dùng.");
            }

            post.MemberId = memberId;
            post.CreatedAt = DateTime.Now;

            // Xử lý ảnh upload -> gán vào PostThumbnailUrl (tên trường phù hợp với model hiện có)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                string uploadPath = Path.Combine(wwwRootPath ?? ".", "images", "uploads");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                post.PostThumbnailUrl = "/images/uploads/" + fileName;
            }

            // Quyết định trạng thái duyệt (model dùng ApproveStatus/PostStatus)
            bool isApproved = User.IsInRole("Admin") || User.IsInRole("Moderator");
            post.ApproveStatus = isApproved ? 1 : 0;
            post.PostStatus = isApproved ? 1 : 0;

            try
            {
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = isApproved
                    ? "Đăng bài viết thành công!"
                    : "Bài viết của bạn đã được gửi và đang chờ duyệt.";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đã xảy ra lỗi khi lưu bài viết: " + ex.Message;
                var categories = await _context.Categories
                                    .OrderBy(c => c.CategoryName)
                                    .ToListAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", post.CategoryId);
                return View(post);
            }
        }
    }
}