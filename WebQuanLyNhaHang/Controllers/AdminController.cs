using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class AdminController : Controller
    {
        private readonly QlnhaHangBtlContext _context;
        private readonly IHubContext<ChatHub> _hubContext;
        public AdminController(QlnhaHangBtlContext context , IHubContext<ChatHub> hubContext) {
            _context = context;
			_hubContext = hubContext;
        }
        public IActionResult Index()
        {
			DoanhThu_LoiNhan doanhThu_LoiNhan = new DoanhThu_LoiNhan(_context);
            return View(doanhThu_LoiNhan);
        }
        #region login logout admin
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string name, string password)
        {
            // Kiểm tra xem name và password có hợp lệ hay không
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Tên đăng nhập và mật khẩu không được để trống.";
                return View();
            }

            // Tìm kiếm nhân viên dựa trên tên đăng nhập và mật khẩu
            var nhanVien = _context.NhanViens.FirstOrDefault(e => e.TaiKhoan == name && e.MatKhau == password);
            // Nếu tìm thấy, chuyển hướng đến trang Admin
            if (nhanVien != null)
            {
                // check quyền 
                ViewModelCheckQuyen check = new ViewModelCheckQuyen(_context);
                int quyen = (int)check.CheckQuyen(nhanVien.NvId); // QUyền của user login
                HttpContext.Session.SetInt32("Quyen", quyen);
                HttpContext.Session.SetInt32("NhanVienId", nhanVien.NvId); // Lưu Id Bàn vào Session
                return RedirectToAction("Index", "Admin");
            }

            // Nếu không tìm thấy, đặt thông báo lỗi và trả về view
            ViewBag.Error = "Tài khoản hoặc mật khẩu không đúng.";
            return View();
        }

        public IActionResult Logout()
        {
            // Chỉ xóa các Session cụ thể
            HttpContext.Session.Remove("Quyen");
            HttpContext.Session.Remove("NhanVienId");

            return RedirectToAction("Login", "Admin");
        }
        #endregion


        public IActionResult Ban()
		{
			ViewModelBan viewModelBan = new ViewModelBan(_context);
			return View(viewModelBan);
		}
		[HttpGet]
		public IActionResult GetFormBuy(int id) // nhận giá trị id bàn
		{
			ViewModelGetFormBuy viewModel = new ViewModelGetFormBuy(_context);
			var donhangs = viewModel.CTDH_Product(id);
			return PartialView("GetFormBuy" , donhangs); 
		}
        [HttpGet]
        public IActionResult ProcessPayment(int BanId)
        {
            // Lấy danh sách các đơn hàng có BanId Gửi tới && Trạng thái đơn Hàng đó phải false(chưa Thanh toán
            var donHangList = _context.DonHangs.Where(e => e.BanId == BanId && e.TrangThai == false).ToList();

            // Cập nhật BanId của mỗi đơn hàng thành null
            foreach (var donHang in donHangList)
            {
                donHang.TrangThai = true;  // Cập Nhật Trạng thái của đơn Hàng
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
            return NoContent();  // Thanh Toán Thành công!
        }  

		public IActionResult DonHangOnline() {
			ViewModelDonHangOnline viewModel = new ViewModelDonHangOnline(_context);
			return View(viewModel.ListDonHang());
		}
		public IActionResult XuLyDonHang(int DonHangID)  // Hàm này Dùng để sử Lý đơn Hàng Online
		{
            ViewModelDonHangOnline viewModel = new ViewModelDonHangOnline(_context);
			var DonHangInfo = viewModel.FindDonHangByID(DonHangID); // Lấy ra đơn hàng và khách bằng id đơn hàng
            return View(DonHangInfo);
		}

		public IActionResult HuyDonHang(int DhId)
		{
			var DH = _context.DonHangs.Find(DhId);
			DH.TrangThai = null;
			DH.VanChuyen = null;
			_context.SaveChanges();
            return RedirectToAction("DonHangOnline", "Admin");
        }
        public IActionResult HoanThanh(int DhId)
        {
            var DH = _context.DonHangs.Find(DhId);
            DH.VanChuyen = true; // Hoàn Thành 
            _context.SaveChanges();
            return RedirectToAction("DonHangOnline", "Admin");
        }

		public IActionResult Bill(int BanId)
		{
			ViewModelGetFormBuy viewModel = new ViewModelGetFormBuy(_context);
            var donhangs = viewModel.CTDH_Product(BanId);
            return View(donhangs);
		}

		public IActionResult DoanhThuSanPham()
		{
			return View();
		}
        [HttpPost]
        public IActionResult CalculateRevenue(DateTime start_date, DateTime end_date)
        {
			// Lấy dữ liệu từ database dựa trên khoảng thời gian
			ViewModelCart viewModel = new ViewModelCart(_context);
			var result = viewModel.ListSanPham(start_date , end_date); //Nhận 2 giá trị để lấy ra số lượng sản phẩm bán được

            return PartialView("TableDoanhThuSanPham", result);  // trả về 1 partialView
        }
    }
}
