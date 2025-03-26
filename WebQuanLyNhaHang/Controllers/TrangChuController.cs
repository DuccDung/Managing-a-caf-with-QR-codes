using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public TrangChuController(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Menu()
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            return View(viewModelMenu);
        }

        public IActionResult CTDH_TrangChu(int ProductID)
        {
            ViewModelProductDetail ViewModel = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var product = ViewModel.FindProductDetaiById(ProductID); // Dữ Liệu product đưa vào View
            return View(product); // chuyền sản phẩm và trạng thái
        }
        public IActionResult GioHang_TrangChu() { 
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            return View(viewModelCart);
        }

        public IActionResult ThanhToan()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add_CTDH_DonHang(int productId, int soluong, string ghichu)
        {
            int DonHangID =(int)HttpContext.Session.GetInt32("DonHangID_ol");
            var CTDH_New = new ChiTietHoaDon  // Tạo 1 chi tiết hóa đơn cho dữ liệu vào
            {
                DhId = DonHangID,
                SoLuong = soluong,
                ProductId = productId,
                Ghichu = ghichu
            }; 
            _qlnhaHangBtlContext.ChiTietHoaDons.Add( CTDH_New );  // add vào cthd
            _qlnhaHangBtlContext.SaveChanges();
            return RedirectToAction("Menu" , "TrangChu");
        }
        
        
        [HttpPost]                
        public IActionResult OrderSuccess(string diachi , string sodienthoai)
        {
            int DhId = (int)HttpContext.Session.GetInt32("DonHangID_ol"); //Lấy id đơn hàng từ session 
            int KhID = (int)HttpContext.Session.GetInt32("CustomerID");
            var DonHang = _qlnhaHangBtlContext.DonHangs.Find(DhId); 
            var KhachHang = _qlnhaHangBtlContext.KhachHangs.Find(KhID);
            if (KhachHang.SoDienThoai == null && KhachHang.DiaChi == null) // TH khách hàng chưa nhập thông tin giao hàng
            {
                KhachHang.DiaChi = diachi;
                KhachHang.SoDienThoai = sodienthoai;
                _qlnhaHangBtlContext.SaveChanges();
            }
            DonHang.GhiChu = "Thông Tin Giao Hàng : Số Điện Thoại: " +KhachHang.SoDienThoai + "Địa Chỉ: " +KhachHang.DiaChi; // gán địa chỉ nếu khách hàng chưa có
            DonHang.TrangThai = true; // => đã đặt Hàng Thanh Công
            DonHang.VanChuyen = false; // Đơn Hàng chưa xác nhận vận chuyển
            DonHang.GioRa = DateTime.Now;

            _qlnhaHangBtlContext.SaveChanges();

            ViewBag.DonHangID = DonHang.DhId;
            ViewBag.TongTien = DonHang.TongTien;
            ViewBag.Ten = KhachHang.TenKhachHang;
            ViewBag.DiaChi = KhachHang.DiaChi;
            ViewBag.Sdt = KhachHang.SoDienThoai;
            return View();  // đưa về form thành công 
        }

        public IActionResult XuLyDonHang()
        {
            // Kiểm tra xem "CustomerID" có tồn tại trong session hay không
            int? customerId = HttpContext.Session.GetInt32("CustomerID");
            if (!customerId.HasValue)
            {
                // Xử lý khi "CustomerID" không tồn tại trong session, ví dụ chuyển hướng tới trang đăng nhập
                return RedirectToAction("Login", "Account");
            }

            // Tạo một đơn hàng mới khi không có đơn hàng nào tồn tại
            var donHangNew = new DonHang
            {
                KhId = customerId.Value,
                TrangThai = false,
                GioVao = DateTime.Now,
                BanId = null
            };

            // Thêm đơn hàng mới vào cơ sở dữ liệu
            _qlnhaHangBtlContext.DonHangs.Add(donHangNew);
            _qlnhaHangBtlContext.SaveChanges();

            // Đặt lại giá trị session "DonHangID_ol" với ID của đơn hàng mới
            HttpContext.Session.SetInt32("DonHangID_ol", donHangNew.DhId);

            // Chuyển hướng người dùng tới trang chủ
            return RedirectToAction("Index", "TrangChu");
        }

    }
}
