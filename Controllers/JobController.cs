// ...existing code...
using BTL_LTW.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using X.PagedList;

namespace BTL_LTW.Controllers
{
    public class JobController : Controller
    {
        private readonly AssociationPortalContext _context;

        public JobController(AssociationPortalContext context)
        {
            _context = context;
        }

        // GET: /Job/
        public async Task<IActionResult> Index(int? page)
        {
            int pageNumber = (page ?? 1);
            int pageSize = 6;

            var jobCategoryId = await _context.Categories
                                        .Where(c => c.CategoryName == "Việc làm")
                                        .Select(c => c.CategoryId)
                                        .FirstOrDefaultAsync();

            if (jobCategoryId == 0)
            {
                var emptyPaged = new StaticPagedList<Post>(new List<Post>(), pageNumber, pageSize, 0);
                return View(emptyPaged);
            }

            var jobsQuery = _context.Posts
                            .Where(p => p.CategoryId == jobCategoryId && p.ApproveStatus == 1)
                            .OrderByDescending(p => p.CreatedAt);

            var totalCount = await jobsQuery.CountAsync();
            var items = await jobsQuery
                            .Skip((pageNumber - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();

            var pagedJobs = new StaticPagedList<Post>(items, pageNumber, pageSize, totalCount);

            return View(pagedJobs);
        }

        // GET: /Job/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var post = await _context.Posts
                .Include(p => p.Member)
                .FirstOrDefaultAsync(m => m.PostId == id);

            if (post == null) return NotFound();

            post.ViewCount = (post.ViewCount ?? 0) + 1;
            _context.Update(post);
            await _context.SaveChangesAsync();

            return View(post);
        }
    }
}
// ...existing code...