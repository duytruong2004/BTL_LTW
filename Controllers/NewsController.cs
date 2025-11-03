using BTL_LTW.Models; // Thay BTL_LTW bằng tên project
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList; // <-- THÊM DÒNG NÀY

namespace BTL_LTW.Controllers // Thay BTL_LTW bằng tên project
{
    public class NewsController : Controller
    {
        private readonly AssociationPortalContext _context;

        public NewsController(AssociationPortalContext context)
        {
            _context = context;
        }

        // GET: /News/
        // GET: /News?page=2
        public async Task<IActionResult> Index(int? page)
        {
            // Lấy ID của Category "Tin tức nổi bật"
            var newsCategoryId = await _context.Categories
                                    .Where(c => c.CategoryName == "Tin tức nổi bật")
                                    .Select(c => c.CategoryId)
                                    .FirstOrDefaultAsync();

            if (newsCategoryId == 0)
            {
                // Xử lý nếu không tìm thấy Category
                return NotFound(); 
            }

            // Lấy danh sách bài Post thuộc Category "Tin tức nổi bật"
            var listNews = _context.Posts
                            .Where(p => p.CategoryId == newsCategoryId)
                            .OrderByDescending(p => p.CreatedAt);

            // Cấu hình phân trang
            int pageNumber = (page ?? 1);
            int pageSize = 6; // Hiển thị 6 bài viết mỗi trang

            // Tạo danh sách đã phân trang
            var pagedNews = await listNews.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedNews);
        }

        // GET: /News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Tìm bài post theo ID
            var post = await _context.Posts
                .Include(p => p.Member) // Lấy thông tin người đăng (nếu cần)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
    }
}