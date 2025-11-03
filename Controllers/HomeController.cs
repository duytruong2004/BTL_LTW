using BTL_LTW.Models;
using BTL_LTW.ViewModels; // <-- Thêm dòng này
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

// Thay BTL_LTW bằng tên namespace project của bạn
namespace BTL_LTW.Controllers
{
    public class HomeController : Controller
    {
        private readonly AssociationPortalContext _context;

        public HomeController(AssociationPortalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tạo ViewModel để chứa cả 2 danh sách
            var viewModel = new HomeViewModel();

            // Lấy 5 tin tức nổi bật (CategoryID = 1)
            viewModel.FeaturedNews = await _context.Posts
                .Where(p => p.CategoryId == 1) // Lọc "Tin tức nổi bật"
                .OrderByDescending(p => p.CreatedAt)
                .Take(6) // Lấy 5 bài
                .ToListAsync();
            
            // Lấy 5 tin tức hội viên (CategoryID = 3)
            viewModel.MemberNews = await _context.Posts
                .Where(p => p.CategoryId == 3) // Lọc "Tin tức hội viên"
                .OrderByDescending(p => p.CreatedAt)
                .Take(6) // Lấy 6 bài
                .ToListAsync();

            // Trả về View với ViewModel
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}