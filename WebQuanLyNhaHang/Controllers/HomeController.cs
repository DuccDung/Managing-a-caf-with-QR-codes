using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Diagnostics;
using WebQuanLyNhaHang.Hubs;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        private readonly IHubContext<ChatHub> _hubContext;

        public HomeController(ILogger<HomeController> logger , QlnhaHangBtlContext qlnhaHangBtlContext , IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
            _hubContext = hubContext;
        }
        #region TRang Login Dành cho khách Hàng từ Xa
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Trang login cho khách hàng quét mã
        // Từ Đường Dẫn Lấy được số bàn rồi vào action này
        public IActionResult Client(int BanId)
        {
            // lấy được dữ liệu bàn
            HttpContext.Session.SetInt32("BanId", BanId); // Lưu Id Bàn vào Session
            // khi quét mã là đẳ đơn 1 lần => tạo 1 đơn hàng 
            var donHangMoi = new DonHang
            {
                GioVao = DateTime.Now,
            };

            _qlnhaHangBtlContext.DonHangs.Add(donHangMoi);
            _qlnhaHangBtlContext.SaveChanges();

            // Sau khi SaveChanges, ID sẽ được cập nhật tự động vào donHangMoi
            var DHID = donHangMoi.DhId; // Lấy ID của đơn hàng
            HttpContext.Session.SetInt32("DhId", DHID);  // lưu đơn hàng id hiện tại vào session
            return View();
        }
        #endregion

        #region Trang Service của client
        public IActionResult Service()
        {
            string? Name = HttpContext.Session.GetString("CustomerName");
            ViewBag.Name = Name;
            return View();
        }
        #endregion

        #region Thông Tin Khách Hàng quét mã
        // Từ trang Client gửi tên và số bàn tới đây để thêm dữ liệu khách hàng vào bàn
        [HttpPost]
        public IActionResult CustomerInfo(string CustomerName)
        {
            HttpContext.Session.SetString("CustomerName", CustomerName);
            int? DhId = HttpContext.Session.GetInt32("DhId");
            var DH = _qlnhaHangBtlContext.DonHangs.Find(DhId);
            DH.GhiChu = "Tên Khách Hàng" + CustomerName;
            return RedirectToAction("Service", "home"); // Đoạn này RedirecAction() về trang tiếp theo
        }
        #endregion

        #region Menu
        public IActionResult Menu()
        {
            // check đơn hàng 
            int DhId = (int)HttpContext.Session.GetInt32("DhId");

            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            if (viewModelMenu.IsOrderCompleted(DhId) == true) // nếu trả về true thì tạo 1 đơn hàng mới
            {
                var donHangMoi = new DonHang
                {
                    GioVao = DateTime.Now,
                };

                _qlnhaHangBtlContext.DonHangs.Add(donHangMoi);
                _qlnhaHangBtlContext.SaveChanges();

                // Sau khi SaveChanges, ID sẽ được cập nhật tự động vào donHangMoi
                var DHID = donHangMoi.DhId; // Lấy ID của đơn hàng
                HttpContext.Session.SetInt32("DhId", DHID);  // lưu đơn hàng id hiện tại vào session
            }

            return View(viewModelMenu);
        }
        #endregion

        #region Lichsu
        public IActionResult LichSu()
        {
            LichSuOderViewModel lichSuOderViewModel = new LichSuOderViewModel(_qlnhaHangBtlContext);
            return View(lichSuOderViewModel);
        }
        #endregion

        public IActionResult GetName(string txtsearch)
        {
            ViewModelMenu viewModelMenu = new ViewModelMenu(_qlnhaHangBtlContext);
            var results = viewModelMenu.ProductsBySearch(txtsearch);
            if(results == null)
            {
                Console.WriteLine("jjdj");
            }
            return PartialView("ProductTable", results);
        }
       

        public IActionResult ProductDetail(int ProductID)
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = HttpContext.Session.GetInt32("DhId"); // Lấy id đơn hàng
            ViewModelProductDetail viewModelProductDetail = new ViewModelProductDetail(_qlnhaHangBtlContext);
            var product = viewModelProductDetail.FindProductDetaiById(ProductID); // Dữ Liệu product đưa vào View
            // 
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId).FirstOrDefault(); // lấy ra đơn hàng Của bàn đag quét
            if(DH != null)
            {
                var CTDH = _qlnhaHangBtlContext.ChiTietHoaDons
             .Where(e => e.ProductId == ProductID && e.DhId == DH.DhId).FirstOrDefault();
                // chi tiết đơn hàng của sản phẩm
                if (CTDH == null)
                {  // trường hợp chưa có Chi tiết đơn hàng thì mặc định cho nó về 1
                    ViewData["SoLuong"] = 1;
                }
                else
                {
                    ViewData["SoLuong"] = CTDH.SoLuong;
                }
            }
            else
            {
                throw new Exception("Lỗi Database trong file Home/ProductDetail");
            }
            return View(product);
        }
        [HttpPost]  // Đón dữ liệu từ chi tiết hóa đơn gửi lên
        public IActionResult CreateProductDetail(int soluong , int productid , string condition , string ghichu) // Thêm CTHD 
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = HttpContext.Session.GetInt32("DhId");
            var DH = _qlnhaHangBtlContext.DonHangs.Where(e => e.DhId == DhId).FirstOrDefault(); // lấy ra đơn hàng Từ cái bàn đó
        
            if(DH != null) // TH: Bàn đã Có đơn hàngn (thì ta tạo Thêm chi tiết hóa đơn)
            {
                var product = _qlnhaHangBtlContext.Products.Find(productid); 
                _qlnhaHangBtlContext.ChiTietHoaDons.Add(new ChiTietHoaDon {
                    DhId = DH.DhId, // mã đơn hàng của bàn đó
                    ProductId = productid, // mã sản phẩm vừa thêm
                    Ghichu ="Trạng Thái: "+condition+" Ghi Chú: "+ghichu, // ghi chú
                    SoLuong = soluong, //số Lượng 
                    ThanhTien = product?.GiaTien * soluong
                });
                _qlnhaHangBtlContext.SaveChanges();
            }
            else
            {
                throw new Exception("Lỗi Tại trang Home/Create");
            }
            return RedirectToAction("Menu" , "Home"); // trở lại trang menu
        }


        public IActionResult Cart(int DhId) // fix
        {
            ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);
            viewModelCart.CTHD_PctByDh(DhId);
            return View(viewModelCart);
        }


        public IActionResult OrderSuccess() // để oder thành công ta kết nối đơn hàng tới bàn
        {
            int? banId = HttpContext.Session.GetInt32("BanId"); // lấy dữ liệu ID bàn từ Sesion
            int? DhId = HttpContext.Session.GetInt32("DhId");
            string? Name = HttpContext.Session.GetString("CustomerName");

            var DH = _qlnhaHangBtlContext.DonHangs.Find(DhId);
            DH.BanId = banId; // Kết nối đơn Hàng Tới bàn
            DH.TrangThai = false; // Gán Trạng thái false cho đơn hàng (Nghĩa là chưa thanh toán)
            DH.GhiChu = Name; // Lưu Tên khách Hàng vào đơn hàng của họ
            _qlnhaHangBtlContext.SaveChanges();
            //dùng phương thức của signalR để nhận biết sự thay đổi của database khi client đặt đơn hàng
            // Phát sự kiện qua SignalR
            _hubContext.Clients.All.SendAsync("OderSuccess");
            return RedirectToAction("Service", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public IActionResult RemoveItem(int id) // id của cthd 
        {
            var CTHD = _qlnhaHangBtlContext.ChiTietHoaDons.Find(id);  // tìm chi tiết hóa đơn
            if (CTHD != null)
            {
                _qlnhaHangBtlContext.ChiTietHoaDons.Remove(CTHD); // xóa chi tiết hóa đơn
                _qlnhaHangBtlContext.SaveChanges();

                //dùng phương thức của signalR để nhận biết sự thay đổi của database
                _hubContext.Clients.All.SendAsync("DatabaseUpdated");
                // Trả về partial view với viewModel
                return NoContent();
            }
            else
            {
                throw new Exception("Lỗi xóa CTDH trong Cart");
            }
;        }

        public IActionResult GetCTHD() // id của cthd 
        {
              // Tạo ViewModel chứa dữ liệu cần thiết để render lại partial view
              ViewModelCart viewModelCart = new ViewModelCart(_qlnhaHangBtlContext);

              // Trả về partial view với viewModel
              return PartialView("CTDHTable", viewModelCart);
        }


        // ================================= trang Login =====================================================================
        [HttpPost]
        public IActionResult CustomerRegister(string username,string Email ,string password) {
            var KhachHang = _qlnhaHangBtlContext.KhachHangs.Where(e => e.TaiKhoan == username).FirstOrDefault();
            if(KhachHang != null)
            {
                TempData["error"] = "Tên Tài Khoản Tài Đã Được Sử Dụng Vui Lòng Đăng Kí Lại!";
                return RedirectToAction("index" , "home");
            }
            else
            {
                // Tạo mới khách hàng
                var khachHang = new KhachHang
                {
                    TenKhachHang = username,
                    TaiKhoan = Email,
                    MatKhau = password
                };
                _qlnhaHangBtlContext.KhachHangs.Add(khachHang);
                _qlnhaHangBtlContext.SaveChanges();
                var DonHang = new DonHang
                {
                    KhId = khachHang.KhId,
                    BanId = null, // vì đây là đơn hàng online
                    GioVao = DateTime.Now,
                    TrangThai = false, // Chưa Thanh Toán => chưa được sử lý
                };
                _qlnhaHangBtlContext.DonHangs.Add(DonHang);


                TempData["success"] = "Tài Khoản Đăng Kí Thành Công!";
                _qlnhaHangBtlContext.SaveChanges();
                return RedirectToAction("Index" , "Home");
            }
        }

        [HttpPost]
        public IActionResult CustomerLogin(string email, string password)
        {
            var khachHang = _qlnhaHangBtlContext.KhachHangs
       .FirstOrDefault(e => e.TaiKhoan == email && e.MatKhau == password);

            if (khachHang != null)
            {
                var id = khachHang.KhId;
                HttpContext.Session.SetInt32("CustomerID", id);
                var DonHang = _qlnhaHangBtlContext.DonHangs.Where(e => e.KhId == id && e.TrangThai == false).FirstOrDefault(); // Tìm đơn Hàng của khách hàng 
                if (DonHang == null) // đay là khi khách đặt hàng thành công đơn hàng sẽ chuyển trạng thái true, và không tồn tại đơn này ở giỏ hàng nữa => tồn tại trong lịch sử đơn hàng đã đặt
                {
                    var DonHangNew = new DonHang  // Tạo ĐƠn Hàng Khi Không có đơn Hàng nào tồn Tại
                    {
                        KhId = id,
                        TrangThai = false,
                        GioVao = DateTime.Now,
                        BanId = null
                    };
                    _qlnhaHangBtlContext.DonHangs.Add(DonHangNew);
                    _qlnhaHangBtlContext.SaveChanges();
                    HttpContext.Session.SetInt32("DonHangID_ol", DonHangNew.DhId); //  Đây là mã đơn Hàng online
                }
                else // đây là đơn hàng chưa được sử lí (Khách mới thêm và giỏ hàng chứ chưa đặt hàng)
                {
                    HttpContext.Session.SetInt32("DonHangID_ol", DonHang.DhId); //  Đây là mã đơn Hàng online
                }
                // Chuyển hướng đến trang thành công
                return RedirectToAction("Index", "TrangChu");
            }
            else
            {
                TempData["error"] = "Tài Khoản Hoặc Mật Khẩu Sai!";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
