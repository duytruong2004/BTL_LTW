// [Nội dung file Controllers/NewsController.cs]
using BTL_LTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace BTL_LTW.Controllers
{
    public class NewsController : Controller
    {
        private readonly AssociationPortalContext _context;

        public NewsController(AssociationPortalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page)
{
    var newsCategoryId = await _context.Categories
                                .Where(c => c.CategoryName == "Tin tức nổi bật")
                                .Select(c => c.CategoryId)
                                .FirstOrDefaultAsync();

    if (newsCategoryId == 0)
    {
        return NotFound();
    }

    var listNews = _context.Posts
                    .Where(p => p.CategoryId == newsCategoryId 
                            && p.ApproveStatus == 1)
                    .OrderByDescending(p => p.CreatedAt);

    int pageNumber = (page ?? 1);
    int pageSize = 6;

    var totalCount = await listNews.CountAsync();
    var items = await listNews
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

    var pagedNews = new X.PagedList.StaticPagedList<Post>(items, pageNumber, pageSize, totalCount);

    return View(pagedNews);
}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Member)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null)
            {
                return NotFound();
            }
            
            // TĂNG VIEW COUNT (NÂNG CAO)
            post.ViewCount = (post.ViewCount ?? 0) + 1;
            _context.Update(post);
            await _context.SaveChangesAsync();


            return View(post);
        }
    }
}