using BTL_LTW.Models; // <-- THÊM DÒNG NÀY ĐỂ SỬA LỖI CS0246

namespace BTL_LTW.ViewModels
{
    public class HomeViewModel
    {
        // Khởi tạo List để sửa cảnh báo CS8618
        public List<Post> FeaturedNews { get; set; } = new List<Post>();
        
        // Khởi tạo List để sửa cảnh báo CS8618
        public List<Post> MemberNews { get; set; } = new List<Post>();
    }
}