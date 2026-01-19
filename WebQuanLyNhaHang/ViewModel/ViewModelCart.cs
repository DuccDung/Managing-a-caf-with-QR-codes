using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelCart
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelCart(QlnhaHangBtlContext context)
        {
            _context = context;
        }
        public List<CTDH_Product> CTHD_PctByDh(int? DhId) // Lấy Ctdh và product join và tìm kiếm nó theo đơn hàng; để phục vụ cart
        {
            var result = from CTHD in _context.ChiTietHoaDons
                         join product in _context.Products
                         on CTHD.ProductId equals product.ProductId
                         where CTHD.DhId == DhId
                         select new CTDH_Product
                         {
                             ProductId = product.ProductId,
                             DhId = DhId,
                             CthdId = CTHD.CthdId,
                             PathPhoto = product.PathPhoto,
                             SoLuong = CTHD.SoLuong,
                             TenSanPham = product.TenSanPham,
                             ThanhTien = product.GiaTien * CTHD.SoLuong,
                             Condition = CTHD.Ghichu
                         };
            foreach (var item in result.ToList()) { 
                var orderDetail = _context.ChiTietHoaDons.Find(item.CthdId);
                var order = _context.DonHangs.Find(DhId);

                if (orderDetail != null) orderDetail.ThanhTien = item.ThanhTien;
                if (order != null) order.TongTien = _context.ChiTietHoaDons
                    .Where(cthd => cthd.DhId == DhId)
                    .Sum(cthd => cthd.ThanhTien);
                _context.SaveChanges();
            }
            if(result == null) // nếu đơn hàng về null mà ép Tolist(); nó sẽ bị bug => ta tạo 1 list mới
            {
                return new List<CTDH_Product>();
            }
            else // result != null trả về list 
            {
                return result.ToList();
            }
        }

        public DonHang TongtienById(int? DhId)
        {
            var result = _context.DonHangs.Find(DhId);
            if (result == null) {
                throw new Exception("Lỗi Không tìm thấy đơn hàng bởi DhId tại ViewModelCart");
            }
            if(result.TongTien == null)
            {
                CTHD_PctByDh(DhId);
                //result.TongTien = 0;
            }
            return result;
        }

        public Boolean checkAddress(int? CustomerID)  // kiểm tra xem khách đã có địa chỉ trong dữ liệu chưa
        {
            var khachHang = _context.KhachHangs.Find(CustomerID);
            if (khachHang.DiaChi == null)
            {
                return false;  // nếu khách hàng chưa nhập địa chỉ ;
            }
            else
            {
                return true;  // có địa chỉ
            }
        }
        public string GetDiaChi(int? CustomerID)
        {
            var khachHang = _context.KhachHangs.Find(CustomerID);
            return khachHang.DiaChi.ToString();
        }
        public string GetSoDienThoai(int? CustomerID)
        {
            var khachHang = _context.KhachHangs.Find(CustomerID);
            return khachHang.SoDienThoai.ToString();
        }

        public List<ProductSalesViewModel> ListSanPham(DateTime startDate, DateTime endDate)
        {
            var result = (from dh in _context.DonHangs
                          join cthd in _context.ChiTietHoaDons on dh.DhId equals cthd.DhId
                          join p in _context.Products on cthd.ProductId equals p.ProductId
                          where dh.TrangThai == true
                                && dh.GioRa != null
                                && dh.GioRa >= startDate
                                && dh.GioRa <= endDate
                                && ((dh.BanId != null) || (dh.KhId != null && dh.VanChuyen == true))
                          group cthd by new { p.ProductId, p.TenSanPham, p.PathPhoto, p.GiaTien } into g
                          select new WebQuanLyNhaHang.ViewModel.ProductSalesViewModel
                          {
                              ProductId = g.Key.ProductId,
                              TenSanPham = g.Key.TenSanPham,
                              PathPhoto = g.Key.PathPhoto,
                              DonGia = g.Key.GiaTien, // Gán DonGia từ Product.GiaTien
                              SoLuong = g.Sum(x => x.SoLuong)
                          }).ToList();

            return result;
        }

        public List<int> DonHangByBanId(int BanId)
        {
            var result = _context.DonHangs.Where(e => e.BanId == BanId && e.TrangThai == false).Select(e => e.DhId);
            return result.ToList();
        }
        public decimal? TongTienBan(int BanId)
        {
                var totalAmount = _context.DonHangs
          .Where(e => e.BanId == BanId && e.TrangThai == false) // Điều kiện lọc
          .Sum(e => e.TongTien); // Cộng dồn giá trị của TongTien
            return totalAmount;
        }
    }
}
